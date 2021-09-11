namespace IntegrationTests.Repository

open System
open System.Linq
open NUnit.Framework
open FsUnit
open Portfolio.Api.MongoRepository
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities


type UserRepositoryTest () =

    let TEST_EMAIL = "test@test.com"
    let TEST_EMAIL_2 = "test_2@test.com"
    let repository = UserRepository(configuration.connectionString) :> IUserRepository

    let delete id = repository.Delete id

    [<SetUp>]
    member this.Setup () =
        delete TEST_EMAIL
        delete TEST_EMAIL_2

    [<TearDown>]
    member this.TearDown () =
        delete(TEST_EMAIL)

    [<Test>]
    member this.``Create & Read`` () =

        let user:User = { Email = TEST_EMAIL; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }
        repository.Create(user)

        let storedUser = repository.Single(TEST_EMAIL)

        Assert.AreEqual(user, storedUser)

    [<Test>]
    member this.Delete () =

        let user:User = { Email = TEST_EMAIL; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }
        repository.Create user

        repository.Delete TEST_EMAIL

        let storedUser = repository.Single TEST_EMAIL

        Assert.IsNull(storedUser)

    [<Test>]
    member this.Update () =

        let user:User = { Email = TEST_EMAIL; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }
        repository.Create user

        let updatedUser:User = { Email = TEST_EMAIL; Username = "username update";  Password = "password update"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = false; PasswordHint = "password hint update"; IsBlocked = true; 
                        }

        repository.Update updatedUser

        let storedUser = repository.Single TEST_EMAIL

        Assert.AreEqual(updatedUser, storedUser)

    [<Test>]
    member this.All () =

        let user_1:User = { Email = TEST_EMAIL; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }

        let user_2:User = { Email = TEST_EMAIL_2; Username = "username";  Password = "password"; CreatedOn = DateTime.UtcNow.Date;
                          IsEmailValidated = true; PasswordHint = "password hint"; IsBlocked = false; 
                        }

        repository.Create user_1
        repository.Create user_2

        let users = repository.All().ToArray()

        users |> should contain user_1
        users |> should contain user_2