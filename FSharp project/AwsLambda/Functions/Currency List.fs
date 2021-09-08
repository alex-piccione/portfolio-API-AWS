namespace Portfolio.Api.Functions.Currency

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open Portfolio.Api.Functions

[<Class>]
type List (repository:ICurrencyRepository) = 
    inherit FunctionBase()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        let list = [
            { Code="EUR"; Name="Euro"}
            { Code="XRP"; Name="Ripple"}
        ]

        base.createOk(Some(list))

