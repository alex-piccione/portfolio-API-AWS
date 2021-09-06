namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core

[<Class>]
type Currency_List () = 

    let createOk (body:string) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 204
        response.Body <- body
        response.Headers.["Content-Type"] = "application/json"

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        // TODO: validate
        // TODO: store in repository

        let body = $@"""{{ ""data"": [
            {{ ""Code"":""EUR"", ""Name"": ""Euro"" }},
            {{ ""Code"":""ETH"", ""Name"": ""Ethereum"" }},
        ]}}"""

        createOk(body)

