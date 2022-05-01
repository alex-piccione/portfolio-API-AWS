// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#new-hosting-model

open Microsoft.AspNetCore.Builder

let url = "http://localhost:5000"
let lamdaFunctionsServerlessFilePath = "../../serverless/serverless.fsharp.yml"

//builder.Host.ConfigureHostConfiguration(configureHost)
let builder = WebApplication.CreateBuilder()
let app = builder.Build()

let functions = LambdaFunctionsMappingGenerator.generateMapping(app,lamdaFunctionsServerlessFilePath )
for f in functions do
    printfn $"Function \"{f.Name}\""     

app.Urls.Add(url)
app.Run()