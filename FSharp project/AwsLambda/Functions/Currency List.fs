namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.entities

[<Class>]
type Currency_List () = 
    inherit FunctionBase()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        let list = [
            { Code="EUR"; Name="Euro"}
            { Code="XRP"; Name="Ripple"}
        ]

        base.createOk(Some(list))

