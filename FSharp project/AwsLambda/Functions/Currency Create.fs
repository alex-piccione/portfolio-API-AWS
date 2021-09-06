namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core

[<Class>]
type Currency_Create () = 

    let createOk () =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 204

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        // TODO: validate
        // TODO: store in repository

        createOk()

