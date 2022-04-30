module LambdaFunctionsMappingGenerator

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Builder
open Portfolio.Core.Entities


let getCurrency (context:HttpContext) =
    let currencies:Currency list = [
        {Code="BTC"; Name="Bitcoin"}
    ]
    context.Response.WriteAsJsonAsync(currencies)
       

let generateMapping (app:WebApplication) =

    app.MapGet("/currency", RequestDelegate getCurrency) |> ignore

