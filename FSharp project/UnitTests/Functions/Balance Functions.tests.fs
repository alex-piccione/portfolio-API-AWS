namespace UnitTests.Functions

open System
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
    member this.``Get <should> return Balance at Today date``() =
        let date = DateTime(2020, 01, 31)
        let balance:Balance = {Date=date; FundsByCurrency=List.empty<FundForCurrency>}
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

        let querystring = Dictionary<string, string>() :> IDictionary<string, string>

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.QueryStringParameters <- querystring

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Get(request, context)
        response.StatusCode |> should equal 409

    [<Test>]
    member this.``Update [when] Date is missing [should] return error``() =
        let balanceLogic = Mock<IBalanceLogic>().Create()
        let functions = BalanceFunctions(balanceLogic)

        let request = Mock<APIGatewayProxyRequest>().Create()
        request.Body <- $@"{{
            ""currencyCode"": ""EUR"",
            ""companyId"": ""aaa"",
            ""Quantity"": 0
        }}"

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Get(request, context)
        response.StatusCode |> should equal 409
