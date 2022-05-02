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
open System.Text.Json

type ``Fund Functions`` () =

    let aDate = DateTime(2000, 1, 1)
    let context = Mock<ILambdaContext>()
                    .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                    .Create()

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
    member this.``GetFund [when] querystring "currency" parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>
        request.QueryStringParameters["from"] <- DateTime(2000, 1, 1).ToString("o") // "20000-01-01T00:00:00.000"

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "currency"

    [<Test>]
    member this.``GetFund [when] querystring "from" parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- dict [("currency","aaa")]        

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "from"

    [<TestCase("")>]
    [<TestCase("20220101")>]
    [<TestCase("aaa")>]
    member this.``GetFund [when] querystring "from" parameter is wrong [should] return error``(from:string) =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- dict[("currency", "aaa"); ("from", from)]

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "from"

    [<Test>]
    member this.``GetFund [should] return list returned by logic``() =
        let currency = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let companyFund1:CompanyFund = {Id=Some("1"); CompanyId="c1"; Quantity=1m; LastUpdateDate=aDate}
                
        let records:CurrencyFundAtDate list = 
            [{
                Date=minDate; CompanyFunds= [companyFund1]; TotalQuantity=1m
            }]
        let balanceLogic = 
            Mock<IBalanceLogic>()
                .Setup(fun l -> l.GetFundOfCurrencyByDate(currency, minDate)).Returns(records)
                .Create()
        let functions = FundFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- dict [("currency", currency); ("from", minDate.ToString("o"))]

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 200   
        //response.Body |> should not' (be Empty)
        Json.JsonSerializer.Deserialize(response.Body) |> should not' (be Null)
   
        verify <@ balanceLogic.GetFundOfCurrencyByDate(currency, minDate) @> once

    [<Test>]
    member this.``Update [should] call Logic function``() =
        let balanceLogic = Mock<IBalanceLogic>()
                              .Setup(fun l -> l.CreateOrUpdate(any()))
                              .Returns(Ok(BalanceUpdateResult.Created))
                              .Create()
                     
        let functions = BalanceFunctions(balanceLogic)

        let request:BalanceUpdateRequest = { Date=DateTime.UtcNow; CurrencyCode="CCC"; CompanyId="C1"; Quantity=1m }
        let proxyRequest = Mock<APIGatewayProxyRequest>().Create()
        proxyRequest.Body <- JsonSerializer.Serialize(request)        

        // execute
        let response = functions.Update(proxyRequest, context)
        response.StatusCode |> should equal 201
    
        let isExpectedRecord = 
            fun (r:BalanceUpdateRequest) -> 
                r.Date |> should equal request.Date
                r.CurrencyCode |> should equal request.CurrencyCode                
                r.CompanyId |> should equal request.CompanyId
                r.Quantity |> should equal request.Quantity
                true

        verify <@ balanceLogic.CreateOrUpdate(is isExpectedRecord) @> once

    [<Test>]
    member this.``Update [when] Logic returns request validation error [should] return error``() =
        let error = Error "invalid request"
        let balanceLogic = Mock<IBalanceLogic>().Setup(fun l -> l.CreateOrUpdate(any())).Returns(error).Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.Body <- $@"{{ }}"

        // execute
        let response = functions.Update(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "invalid request"

        verify <@ balanceLogic.CreateOrUpdate(any()) @> once