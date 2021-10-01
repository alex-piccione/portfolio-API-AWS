module UnitTests.Functions

open System
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open Portfolio.Api.Functions
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core

open SessionManager


type ``User Functions`` () =

    let testUser:User = 
        { Username="test"; Email="test@test.com"; Password="password"; PasswordHint="password hint"; 
            CreatedOn=DateTime.UtcNow; IsEmailValidated=false; IsBlocked=false }

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
    member this.``Create <when> Email exists <should> return Error``() =
                
        let email = "EmaiL@Test.COM"
        let existingEmail = email.ToLowerInvariant()
        let user:User = { testUser with Email = existingEmail }

        let repository:IUserRepository =
            Mock<IUserRepository>()
                .Setup(fun rep -> rep.Single email).Returns(Some user)
                .Create()

        let sessionManager = Mock<ISessionManager>().Create()

        let functions:UserFunctions = UserFunctions(repository, sessionManager)

        // execute
        let response = functions.Create(this.emulateApi user)

        response |> should not' (be Null)
        response.StatusCode |> should equal 409
        response.Body |> should contain "An user with this same email already exists."

        (*
    [<Test>]
    member this.TestUserRepository() =

        let email = "aaa"
        let user:User = { testUser with Email = email }

        let repository:IUserRepository =
            Mock<IUserRepository>()
                .Setup(fun rep -> rep.Single email).Returns(Some user)
                .Create()

        let loadUser = repository.Single email

        ()

        *)
  

type IWriter =
    abstract member Write: string -> string

type ISpecialWriter =
    inherit IWriter

type ClassToTest(writer:ISpecialWriter) =
    member this.DoSomething text = 
        writer.Write text


// open Foq.Linq
[<Test>]
let ``Mock_should_work_with_exact_input``() =
    let input = "aaa"
    let output = "bbb"
    let specialWriter:ISpecialWriter = 
        Mock<ISpecialWriter>()
            //.Setup(fun w -> w.Write (It.any()) ).Returns("bbb")
            .Setup(fun w -> w.Write input).Returns(output)
            .Create()

    let classToTest = ClassToTest(specialWriter)

    // execute
    let result = classToTest.DoSomething("aaa")

    result |> should equal "bbb"

type ClassToTest_2(userRepository:IUserRepository) =
    member this.DoSomething email = 
        userRepository.Single email

[<Test>]
let ``Mock_should_work_when_use_IUserRepository``() =
    
    //let user:User = { testUser }
    let email = "EmaiL@Test.COM"
    let user:User = 
        { Username="test"; Email="test@test.com"; Password="password"; PasswordHint="password hint"; 
            CreatedOn=DateTime.UtcNow; IsEmailValidated=false; IsBlocked=false }

    let userRepository:IUserRepository = 
        Mock<IUserRepository>()
            .Setup(fun rep -> rep.Single (It.IsAny<string>()) ).Returns(Some user)
            //.Setup(fun rep -> rep.Single(email) ).Returns(Some user)            
            .Create()

    let classToTest = ClassToTest_2(userRepository)

    // execute
    let result = classToTest.DoSomething("aaa")

    result.IsSome |> should be True

