namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core

[<Class>]
type Currency_List () = 

    let createOk (body:string) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 204

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        // TODO: validate
        // TODO: store in repository

        let body = $@"""[
            {{ ""Code"":""EUR"", ""Name"": ""Euro"" }},
            {{ ""Code"":""ETH"", ""Name"": ""Ethereum"" }},
        ]"""

        createOk(body)

