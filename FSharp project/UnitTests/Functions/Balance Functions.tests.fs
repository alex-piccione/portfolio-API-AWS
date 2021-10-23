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
open Portfolio.Core
open Portfolio.Core.Entities
open Portfolio.Core.Logic
open Portfolio.Api.Functions


type ``Balance Functions`` () =

    let TEST_ID = "test-123"

    let testCompany:Company = { Id=TEST_ID; Name="Company A"; Types=[CompanyType.Bank] }

    let getLogic() = Mock<IBalanceLogic>().Create()
    //let getRepository() = Mock<ICompanyRepository>().Create()

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
    member this.``Get current Balance``() =

        let json = @"{
          ""Name"": ""Test"",
          ""Types"": [""Bank""]
        }"

        let functions = BalanceFunctions(getLogic())

        let querystring = Dictionary<string, string>() :> IDictionary<string, string>
        querystring.["base-currency"] <- "EUR"
        let request =
            Mock<APIGatewayProxyRequest>()
                        .SetupPropertyGet(fun r -> r.QueryStringParameters).Returns(querystring)
                        .Create()

        request.Body <- ""
        request.QueryStringParameters <- querystring

        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock.Of<ILambdaLogger>())
                          .Create()

        // execute
        let response = functions.Get(request, context)
        ()
        //item.Name |> should equal "Test"
        //item.Types |> should equivalent [CompanyType.Bank]
