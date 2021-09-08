namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Core
open Portfolio.Api.MongoRepository

[<Class>]
type Get (repository:ICurrencyRepository) =
    inherit FunctionBase()

    new () = 
        let repolsitory = CurrencyRepository()
        Get(repolsitory)

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) =
        match request.QueryStringParameters.TryGetValue("code") with
        | (true, id) -> repository.Get id
        | _ -> failwith @"Missing querystring parameter ""code""."
