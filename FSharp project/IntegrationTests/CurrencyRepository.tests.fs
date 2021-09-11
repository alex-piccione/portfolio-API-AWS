namespace IntegrationTests.Repository

open NUnit.Framework
open Portfolio.Api.Core
open Portfolio.Api.MongoRepository
open Portfolio.Api.Core.Entities
open FsUnit

type ``Currency Repository`` () =

    let repository = CurrencyRepository(configuration.connectionString) :> ICurrencyRepository
    let TEST_CODE = "TEST1"
    let TEST_CODE_2 = "TEST2"

    let delete id = repository.Delete id

    [<SetUp>]
    member this.Setup () =
        delete TEST_CODE
        delete TEST_CODE_2

    [<TearDown>]
    member this.TearDown () =
        delete TEST_CODE
        delete TEST_CODE_2

    [<Test>]
    member this.``Create & Read`` () =
        let currency:Currency = { Code=TEST_CODE; Name="Currency Test" }
        repository.Create(currency)
        let storedCurrency = repository.Single(TEST_CODE)
        storedCurrency |> should equal (Some currency)