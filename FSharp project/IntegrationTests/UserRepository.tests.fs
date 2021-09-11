namespace IntegrationTests.Repository

open NUnit.Framework
open Portfolio.Api.MongoRepository
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open System
open FsUnit

type UserRepositoryTest () =

    let TEST_EMAIL = "test@test.com"
    let repository = UserRepository(configuration.connectionString) :> IUserRepository

    let delete id = repository.Delete id

    [<SetUp>]
    member this.Setup () =
        delete(TEST_EMAIL)

    [<TearDown>]
    member this.TearDown () =
        delete(TEST_EMAIL)

    [<Test>]
    member this.Create () =

        let user:User = { Email = TEST_EMAIL; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }
        repository.Create(user)

        let savedUser = repository.Single(TEST_EMAIL)

        Assert.AreEqual(user, savedUser)
