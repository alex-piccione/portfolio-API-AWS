namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core


[<Class>]
type Get () =
    inherit CurrencyFunction()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) =
        match request.QueryStringParameters.TryGetValue("code") with
        | (true, id) -> this.Repository.Get id
        | _ -> failwith @"Missing querystring parameter ""code""."
