namespace UnitTests.Functions

open System
open Microsoft.FSharp.Core
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open Portfolio.Core.Logic
open Portfolio.Api.Functions
open System.Collections.Generic


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

       let querystring = Dictionary<string, string>() 
       querystring.["base-currency"] <- "EUR"
       let request:APIGatewayProxyRequest = Mock<APIGatewayProxyRequest>()
                                                .SetupPropertyGet(fun r -> r.QueryStringParameters).Returns(querystring)
                                                .Create()
       let context = Mock.Of<ILambdaContext>()

       // execute
       let response = functions.Get(request, context)
       ()
       //item.Name |> should equal "Test"
       //item.Types |> should equivalent [CompanyType.Bank]