module LambdaFunctionsMappingGenerator

open System.IO
open System.Linq
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open YamlDotNet.Serialization
open Portfolio.Core.Entities
open System.Collections.Generic
open System

let getCurrency (context:HttpContext) =
    let currencies:Currency list = [
        {Code="BTC"; Name="Bitcoin"}
    ]
    context.Response.WriteAsJsonAsync(currencies)
  
type data = 
    abstract member functions:obj with get 

type LambdaFunction (name:string, httpPath:string, httpMethod:string, clazz:Type, methodName:string) =
    let methodInfo = clazz.GetMethod(methodName)

    member this.Name = name 
    member this.HttpPath = httpPath 
    member this.HttpMethod = httpMethod
    member this.Class = clazz 
    member this.MethodName = methodName
    member this.MethodInfo = methodInfo

let readFunctions serverLessFunctionsFile = 
    let text = File.ReadAllText(serverLessFunctionsFile)
    let deserializer = 
        DeserializerBuilder()
            //.WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
            .Build()
            
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

            let values = handler.Split("::")   // Assembly::Namespace.Class::Method
            let assembly = System.Reflection.Assembly.Load(values[0])
            let clazz = assembly.DefinedTypes.ToList().Find(fun t -> t.FullName = values[1])
            let methodName = values[2]

            let events = functionItem["events"] :?> List<obj>
            let http = (events[0] :?> IDictionary<obj, obj>)["http"] :?> IDictionary<obj, obj>

            let httpMethod = http["method"].ToString()
            let httpPath = http["path"].ToString()

            LambdaFunction(functionName, httpPath, httpMethod, clazz, methodName)]

let generateMapping (app:WebApplication, serverLessFunctionsFile) =

    let functions = readFunctions serverLessFunctionsFile
    
    for f in functions do
        printf $"Function \"{f.Name}\"" 
        app.MapGet("/currency", RequestDelegate getCurrency) |> ignore

