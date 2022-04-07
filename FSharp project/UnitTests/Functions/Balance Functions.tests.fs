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


type ``Balance Functions`` () =

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
    member this.``Get [should] return Balance at Today date``() =
        let date = DateTime(2020, 01, 31)
        let lastUpdateDate = date.AddMonths(-1)
        let balance:Balance = {Date=date; FundsByCurrency=List.empty<FundForCurrency>; LastUpdateDate=lastUpdateDate}
        let balanceLogic = Mock<IBalanceLogic>()
                               .SetupFunc(fun l -> l.GetBalance(It.IsAny<DateTime>())).Returns(balance)
                               .Create()
        let functions = BalanceFunctions(balanceLogic)

        let querystring = Dictionary<string, string>() :> IDictionary<string, string>
        querystring.["base-currency"] <- "EUR"
        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- querystring

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Get(request, context)

        response.StatusCode |> should equal 200
        response.Body |> should not' (be Empty)

        let returnedBalance = System.Text.Json.JsonSerializer.Deserialize(response.Body)
        returnedBalance |> should not' (be Null)
        let today = DateTime.UtcNow.Date
        verify <@ balanceLogic.GetBalance(today) @> once

    [<Test>]
    member this.``Get [when] querystring parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Get(request, context)
        response.StatusCode |> should equal 409
        test_helper.verifyResponseContainsError response "Parameter \"base-currency\" not found in querystring"
        
    [<Test>]
    member this.``GetFund [should] return list returned by logic ``() =
        let currency = "AAA"
        let limit = Some 10
        let aFund:FundAtDate = { Id="1"; Date=DateTime.Today; CurrencyCode=currency; FundCompanyId="c1"; Quantity=1m; LastChangeDate=DateTime.Today} 
        let records:FundAtDate list = [
            aFund
            { aFund with Id="2"}
            { aFund with Id="3"}
            ]
        let balanceLogic = 
            Mock<IBalanceLogic>()
                .Setup(fun l -> l.GetFund(currency, limit)).Returns(records)
                .Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>
        request.QueryStringParameters.Add("currency", currency)
        request.QueryStringParameters.Add("limit", limit.Value.ToString())

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 200   
        //response.Body |> should not' (be Empty)
        Json.JsonSerializer.Deserialize(response.Body) |> should not' (be Null)
       
        verify <@ balanceLogic.GetFund(currency, limit) @> once

    [<Test>]
    member this.``GetFund [when] querystring parameter is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- Dictionary<string, string>() :> IDictionary<string, string>

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.GetFund(request, context)
        response.StatusCode |> should equal 409

    [<Test>]
    member this.``Update [should] call Logic function``() =
        let balanceLogic = Mock<IBalanceLogic>()
                              .Setup(fun l -> l.CreateOrUpdate(any()))
                              .Returns(Ok(BalanceUpdateResult.Created))
                              .Create()
                         
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.Body <- $@"{{            
            ""currencyCode"": ""CCC"",
            ""date"": ""2000-12-31"",
            ""companyId"": ""AAA"",
            ""quantity"": 11
        }}"

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Update(request, context)
        response.StatusCode |> should equal 201
        //response.Body |> should contain "Fund request"
        
        let isExpectedRecord = 
            fun (r:BalanceUpdateRequest) -> 
                // r.Date
                r.CurrencyCode |> should equal "CCC"
                r.Quantity |> should equal 11
                r.CompanyId |> should equal "AAA"
                true

        verify <@ balanceLogic.CreateOrUpdate(is isExpectedRecord) @> once

    [<Test>]
    member this.``Update [when] Logic returns request validation error [should] return error``() =
        let error = Error "invalid request"
        let balanceLogic = Mock<IBalanceLogic>().Setup(fun l -> l.CreateOrUpdate(any())).Returns(error).Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.Body <- $@"{{ }}"

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Update(request, context)
        response.StatusCode |> should equal 409
        response.Body |> should contain "invalid request"
