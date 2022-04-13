namespace UnitTests.Functions

open System
open System.Text
open System.Collections.Generic
open Microsoft.FSharp.Core
open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core.Entities
open Portfolio.Core.Logic
open Portfolio.Api.Functions


type ``Fund Functions`` () =

    member this.emulateApi<'T> (item:'T) =
        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock<ILambdaLogger>().Create()) 
                          .Create()
        let request = APIGatewayProxyRequest()
        request.Body <- System.Text.Json.JsonSerializer.Serialize<'T> item
        (request, context)


    [<SetUp>]
    member this.Setup () =
        ()  

       
    [<Test>]
    member this.``GetFund [should] return list returned by logic ``() =
        let currency = "AAA"
        let minDate = DateTime(2000, 1, 1)
        let aFund:FundAtDate = { Id="1"; Date=DateTime.Today; CurrencyCode=currency; FundCompanyId="c1"; Quantity=1m; LastChangeDate=DateTime.Today} 
        let records:FundAtDate list = [
            aFund
            { aFund with Id="2"}
            { aFund with Id="3"}
            ]
        let balanceLogic = 
            Mock<IBalanceLogic>()
                .Setup(fun l -> l.GetFund(currency, minDate)).Returns(records)
                .Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>
        request.QueryStringParameters.Add("currency", currency)
        request.QueryStringParameters.Add("from", minDate.ToString("o"))

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 200   
        //response.Body |> should not' (be Empty)
        Json.JsonSerializer.Deserialize(response.Body) |> should not' (be Null)
       
        verify <@ balanceLogic.GetFund(currency, minDate) @> once

    [<Test>]
    member this.``GetFund [when] querystring "currency" parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>
        request.QueryStringParameters["from"] <- DateTime(2000, 1, 1).ToString("o") // "20000-01-01T00:00:00.000"

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "currency"

    [<Test>]
    member this.``GetFund [when] querystring "from" parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>
        request.QueryStringParameters["currency"] <- "aaa"
        

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "from"