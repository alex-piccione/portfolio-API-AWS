module UnitTests.Functions

open System
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open Portfolio.Api.Functions
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Foq.Linq


type ``User Functions`` () =

    let testUser:User = 
        { Username="test"; Email="test@test.com"; Password="password"; PasswordHint="password hint"; 
            CreatedOn=DateTime.UtcNow; IsEmailValidated=false; IsBlocked=false }

    member this.emulateApi<'T> (user:'T) =
        let context = Mock<ILambdaContext>()
                          .SetupPropertyGet(fun c -> c.Logger).Returns( Mock<ILambdaLogger>().Create()) 
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

        //let aa = repository.Single(email)

        let functions:UserFunctions = UserFunctions(repository)

        //(fun _ -> functions.Create <| this.emulateApi user |> ignore)
        let a = functions.Create( this.emulateApi user)

        (fun _ -> functions.Create( this.emulateApi user) |> ignore)
        |> should throw typeof<Exception>
