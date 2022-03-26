namespace UnitTests.Functions

open System
open Microsoft.FSharp.Core
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core.Entities
open Portfolio.Core.Logic
open Portfolio.Api.Functions


type ``Currency Functions`` () =

    let testCurrency:Currency = { Code="test"; Name="UnitTest" }
    //let getLogic () = Mock.Of<ICurrencyLogic>()
 
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
    member this.``Create`` () =
        let item = {testCurrency with Code="AAA"}

        let s:unit = ()
        let logic = Mock<ICurrencyLogic>()
                        .SetupFunc(fun rep -> rep.Create item)
                        .Returns(Ok item)
                        .Create()

        let functions = CurrencyFunctions(logic)

        // execute
        let response = functions.Create(this.emulateApi item) 
        response.StatusCode |> should equal 201

    [<Test>]
    member this.``Create [when] code already exuists`` () =
        let item = {testCurrency with Code="AAA"}
        let message = "Already exists"

        let s:unit = ()
        let logic = Mock<ICurrencyLogic>()
                        .SetupFunc(fun rep -> rep.Create item)
                        .Returns(Error message)
                        .Create()

        let functions = CurrencyFunctions(logic)

        // execute
        let response = functions.Create(this.emulateApi item) 
        response.StatusCode |> should equal 409
        response.Body |> should equal message

     (*
    [<Test>]
    member this.``Update`` () =
        let item = {testCurrency with Name="Test Update"}

        let s:unit = ()
        let logic = Mock<ICurrencyLogic>()
                        //.SetupFunc(fun rep -> rep.Single item.Code).Returns(Some item)
                        .SetupFunc(fun rep -> rep.Update item).Returns(Ok item)
                        .Create()

        let functions = CurrencyFunctions(logic)

        // execute
        let response = functions.Update(this.emulateApi item) 
        response.StatusCode |> should equal 200



    [<Test>]
    member this.``Deserialize``() =

       let json = @"{
         ""Name"": ""Test"",
         ""Types"": [""Bank""]
       }"

       let functions = CompanyFunctions(getLogic())

       // execute
       let item:Company = functions.Deserialize json
       item.Name |> should equal "Test"
       item.Types |> should equivalent [CompanyType.Bank]

    [<Test>]
    member this.``Deserialize <with> more than one company types``() =

       let json = @"{
         ""Name"": ""Test"",
         ""Types"": [""Bank"", ""Exchange""]
       }"

       let functions = CompanyFunctions(getLogic())

       // execute
       let item:Company = functions.Deserialize json
       item.Name |> should equal "Test"
       item.Types |> should equivalent [CompanyType.Bank; CompanyType.Exchange]

       *)