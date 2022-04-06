module test_helper

open System.Text
open FsUnit
open Amazon.Lambda.APIGatewayEvents

let verifyResponseContainsError (response:APIGatewayProxyResponse) message =

    let data = Json.JsonSerializer.Deserialize<string list>(response.Body)
    data |> should contain message

