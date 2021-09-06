﻿namespace Portfolio.Lambda.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core

[<Class>]
type Currency_List () = 

    let createOk (body:string) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 204
        response.Body <- body
        response.Headers.["Content-Type"] <- "application/json"
        response

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        context.Logger.LogLine("Handle start")

        // TODO: validate
        // TODO: store in repository

        let body = $@"""{{ ""data"": [
            {{ ""Code"":""EUR"", ""Name"": ""Euro"" }},
            {{ ""Code"":""ETH"", ""Name"": ""Ethereum"" }},
        ]}}"""

        context.Logger.LogLine("Handle end")

        createOk(body)

