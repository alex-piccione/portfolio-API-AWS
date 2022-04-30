// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#new-hosting-model

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
//open Microsoft.AspNetCore  WebHost

let url = "http://localhost:5000"
let lamdaFunctionsServerlessFilePath = "../../serverless/serverless.fsharp.yml"

let functions = LambdaFunctionsMappingGenerator.readFunctions lamdaFunctionsServerlessFilePath

for f in functions do
    printfn $"Function \"{f.Name}\"" 
    //app.MapGet("/currency", RequestDelegate getCurrency) |> ignore


(*
let configureHost (options:IConfigurationBuilder) =
    let configuration:IConfiguration = {
    }
    options.AddConfiguration(configuration)
*)

//builder.Host.ConfigureHostConfiguration(configureHost)
let builder = WebApplication.CreateBuilder()
let app = builder.Build()

let getHome (context:HttpContext) =    
    context.Response.WriteAsync("hello")

app.MapGet("/", RequestDelegate getHome) |> ignore
LambdaFunctionsMappingGenerator.generateMapping(app, lamdaFunctionsServerlessFilePath)

app.Urls.Add(url)
app.Run()