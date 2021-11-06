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


type ``Company Functions`` () =

    let testCompany:Company = { Id="test"; Name="UnitTest"; Types=[CompanyType.Bank]}
    let getLogic() = Mock<ICompanyLogic>().Create()
    let getRepository() = Mock<ICompanyRepository>().Create()

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

    (*
    [<Test>]
    member this.``Update`` () =

        let itemToUpdate:Company = {testCompany with Name="Test Update"; Types=[CompanyType.Exchange]}

        let s:unit = ()
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.Update(itemToUpdate))
                             .Returns(s)
                             .Create()

        let functions = CompanyFunctions(repository)

        // execute
        let response = functions.Update(this.emulateApi itemToUpdate) 
        response.StatusCode |> should equal 200
        *)

 
    (*[<Test>]
    member this.``Create`` () =

        let itemToUpdate:Company = {testCompany with Id="aaa"}

        let s:unit = ()
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.Create(itemToUpdate))
                             .Returns(s)
                             .Create()



        let functions = CompanyFunctions(getLogic(), repository)

        // execute
        let response = functions.Create(this.emulateApi() itemToUpdate) 
        response.StatusCode |> should equal 200
        *)


    [<Test>]
    member this.``Deserialize``() =

       let json = @"{
         ""Name"": ""Test"",
         ""Types"": [""Bank""]
       }"

       let functions = CompanyFunctions(getLogic(), getRepository())

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

       let functions = CompanyFunctions(getLogic(), getRepository())

       // execute
       let item:Company = functions.Deserialize json
       item.Name |> should equal "Test"
       item.Types |> should equivalent [CompanyType.Bank; CompanyType.Exchange]
