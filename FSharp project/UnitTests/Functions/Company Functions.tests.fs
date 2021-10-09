namespace UnitTests.Functions

open System
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open Portfolio.Api.Functions



type ``Company Functions`` () =

    let testUser:User = 
        { Username="test"; Email="test@test.com"; Password="password"; PasswordHint="password hint"; 
            CreatedOn=DateTime.UtcNow; IsEmailValidated=false; IsBlocked=false }

    let mutable repository = Mock<ICompanyRepository>().Create()

    member this.emulateApi<'T> (user:'T) =
        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns(Mock<ILambdaLogger>().Create()) 
                          .Create()
        let request = APIGatewayProxyRequest()
        request.Body <- System.Text.Json.JsonSerializer.Serialize<'T> user
        (request, context)

    

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.``Deserialize``() =

       let json = @"{
         ""Name"": ""Test"",
         ""Types"": [""Bank""]
       }"

       let functions = CompanyFunctions(repository)

       let item:Company = functions.Deserialize json
       item.Name |> should equal "Test"
       item.Types |> should equivalent [CompanyType.Bank]

    [<Test>]
    member this.``Deserialize <with> more than one company types``() =

       let json = @"{
         ""Name"": ""Test"",
         ""Types"": [""Bank"", ""Exchange""]
       }"

       let functions = CompanyFunctions(repository)

       let item:Company = functions.Deserialize json
       item.Name |> should equal "Test"
       item.Types |> should equivalent [CompanyType.Bank; CompanyType.Exchange]
