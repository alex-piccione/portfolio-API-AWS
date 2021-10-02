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


type MyObject(repository:IUserRepository) =

    member this.Single email = 
        try
            if repository.Single(email).IsSome then
                true
            else
                false
        with exc ->
            failwith $"Something went wrong. {exc}"

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
        let searchedEmail = email.ToLowerInvariant()
        let user:User = { testUser with Email = email }

        let userRepository =
            Mock<IUserRepository>()
                .Setup(fun rep -> rep.Single searchedEmail).Returns(Some user)
                .Create()

        let sessionManager = Mock<ISessionManager>().Create()

        let functions:UserFunctions = UserFunctions(userRepository, sessionManager)

        // execute
        let response = functions.Create(this.emulateApi user)

        response |> should not' (be Null)
        response.StatusCode |> should equal 409
        response.Body |> should contain "An user with this same email already exists."

