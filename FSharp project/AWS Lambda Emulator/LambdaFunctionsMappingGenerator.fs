module LambdaFunctionsMappingGenerator

open System
open System.IO
open System.Linq
open System.Collections.Generic
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open YamlDotNet.Serialization
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open AwsLambdaDummies

type LambdaFunction (name:string, httpPath:string, httpMethod:string, clazz:Type, methodName:string) =
    let methodInfo = clazz.GetMethod(methodName)
    let parameterlessConstructor = clazz.GetConstructor(System.Type.EmptyTypes)
    let functionInstance = parameterlessConstructor.Invoke([||])

    member this.Name = name 
    member this.HttpPath = httpPath 
    member this.HttpMethod = httpMethod
    member this.Class = clazz 
    member this.Function = functionInstance
    member this.MethodName = methodName
    member this.MethodInfo = methodInfo


let readFunctions serverLessFunctionsFile = 
    let text = File.ReadAllText(serverLessFunctionsFile)
    let deserializer = DeserializerBuilder().Build()
            
    let lambda = deserializer.Deserialize<IDictionary<string, obj>>(text);

    match lambda.ContainsKey("functions") with 
    | false -> failwith "Cannot find \"functions\" in yaml file"
    | _ ->
        [for kv in lambda["functions"] :?> IDictionary<obj, obj> do            
            let functionName = kv.Key.ToString()
            let replacePath (handler:string) =                 
                handler // TODO: load variables
                    .Replace("${self:provider.environment.currencyFunctions}", "Portfolio.Api::Portfolio.Api.Functions.CurrencyFunctions")
                    .Replace("${self:provider.environment.companyFunctions}", "Portfolio.Api::Portfolio.Api.Functions.CompanyFunctions")
                    .Replace("${self:provider.environment.balanceFunctions}", "Portfolio.Api::Portfolio.Api.Functions.BalanceFunctions")
                    .Replace("${self:provider.environment.fundFunctions}", "Portfolio.Api::Portfolio.Api.Functions.FundFunctions")
                    .Replace("${self:provider.environment.userFunctions}", "Portfolio.Api::Portfolio.Api.Functions.UserFunctions")

            let functionItem = kv.Value :?> IDictionary<obj, obj>
            let handler = replacePath (functionItem["handler"].ToString())          

            let values = handler.Split("::") // Assembly::Namespace.Class::Method
            let assembly = System.Reflection.Assembly.Load(values[0])
            let clazz = assembly.DefinedTypes.ToList().Find(fun t -> t.FullName = values[1])
            let methodName = values[2]

            let events = functionItem["events"] :?> List<obj>
            let http = (events[0] :?> IDictionary<obj, obj>)["http"] :?> IDictionary<obj, obj>

            let httpMethod = http["method"].ToString()
            let httpPath = http["path"].ToString()

            LambdaFunction(functionName, httpPath, httpMethod, clazz, methodName)]

let generateCall (f:LambdaFunction) =   
    let logger = LambdaLogger() 

    let call (context:HttpContext) =
        let lambdaResponse = 
            try 
                let request:APIGatewayProxyRequest = APIGatewayProxyRequest()
                request.HttpMethod <- f.HttpMethod
                let qs = context.Request.Query.ToDictionary( (fun kv -> kv.Key), (fun kv -> String.Join(',', kv.Value.ToArray()) ))
                if qs.Count > 0 then request.QueryStringParameters <- qs
                request.Body <- ((new StreamReader(context.Request.Body)).ReadToEndAsync()).Result
                
                let lambdaContext:ILambdaContext = LambdaContext(f.Name, logger)
                
                f.MethodInfo.Invoke(f.Function, [|request; lambdaContext|]) :?> APIGatewayProxyResponse

            with exc -> failwith $"{f.MethodName} caused an error. {exc}"     
            

        context.Response.StatusCode <- lambdaResponse.StatusCode    
        if lambdaResponse.Headers <> null then
            for h in lambdaResponse.Headers do context.Response.Headers[h.Key] <- h.Value        
        context.Response.WriteAsync (lambdaResponse.Body)      
        
    call

let generateMapping (app:WebApplication, serverLessFunctionsFile) =
    let functions = readFunctions serverLessFunctionsFile 
   
    for f in functions do
        printfn $"Function \"{f.Name}\": {f.HttpMethod} {f.HttpPath}" 
        app.MapMethods("/" + f.HttpPath, [f.HttpMethod], RequestDelegate (generateCall f)) |> ignore        

    let pageHtmlTemplate = File.ReadAllText("Functions page template.html")
    let functionHtmlTemplate = File.ReadAllText("Function item template.html")
    let functionsHtml = List.fold (
        fun s (f:LambdaFunction) -> s + functionHtmlTemplate
                                            .Replace("{name}", f.Name)
                                            .Replace("{method}", f.HttpMethod)
                                            .Replace("{path}", f.HttpPath)
                                            ) "" functions
    let getFunctions (context:HttpContext) =
        context.Response.WriteAsync (pageHtmlTemplate.Replace("{functions}", functionsHtml))
    app.MapGet("/", RequestDelegate getFunctions) |> ignore

    functions