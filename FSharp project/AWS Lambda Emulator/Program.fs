// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#new-hosting-model

//open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

//open Microsoft.AspNetCore  WebHost

let url = "http://localhost:5000"

//WebHost
let builder = WebApplication.CreateBuilder()

(*
let configureHost (options:IConfigurationBuilder) =
    let configuration:IConfiguration = {
    }
    options.AddConfiguration(configuration)
*)

//builder.Host.ConfigureHostConfiguration(configureHost)
let app = builder.Build()

let getHome (context:HttpContext) =    
    context.Response.WriteAsync("hello")

app.MapGet("/", RequestDelegate getHome) |> ignore
LambdaFunctionsMappingGenerator.generateMapping(app)

app.Urls.Add(url)
app.Run()