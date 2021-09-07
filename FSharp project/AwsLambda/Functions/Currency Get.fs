namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Core

[<Class>]
type Get (repository:ICurrencyRepository) =
    inherit FunctionBase()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) =
        match request.QueryStringParameters.TryGetValue("code") with
        | (true, id) -> repository.Get id
        | _ -> failwith @"Missing querystring parameter ""code""."
