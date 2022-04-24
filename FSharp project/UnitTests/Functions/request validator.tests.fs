module UnitTests.Functions.request_validator_test

open NUnit.Framework
open Amazon.Lambda.APIGatewayEvents
open System.Collections.Generic
open request_validator
open FsUnit

[<Test>]
let ``ValidateRequest [when] no rules [should] return no errors`` () =    
    let request = APIGatewayProxyRequest()
    request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>    
    request_validator.ValidateRequest request List.Empty |> should be Empty

[<Test>]
let ``ValidateRequest [When] must exist [should] return error`` () =

    let querystring = Dictionary<string, string>() :> IDictionary<string, string>
    //querystring.["base-currency"] <- "EUR"
    let request = APIGatewayProxyRequest()
    request.QueryStringParameters <- querystring

    let rule = ParameterMustExist "aaa"
    
    let errors = request_validator.ValidateRequest request [rule]
    errors |> should not' (be Empty)
    errors.Head.Parameter |> should equal "aaa"
    errors.Head.Message |> should equal "Parameter \"aaa\" not found in querystring"
