namespace IntegrationTests.Repository

open NUnit.Framework
open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
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

    [<Test>]
    member this.``ExistsWithCode`` () =
        let currency:Currency = { Code=TEST_CODE; Name="Currency Test" }
        repository.Create(currency)
        repository.ExistsWithCode(TEST_CODE) |> should be True
        repository.ExistsWithCode(TEST_CODE + "-000") |> should be False

    [<Test>]
    member this.``ExistsWithName`` () =
        let currency:Currency = { Code=TEST_CODE; Name="Currency AAA" }
        repository.Create(currency)
        repository.ExistsWithName("Currency AAA") |> should be True
        repository.ExistsWithName("Currency BBB") |> should be False