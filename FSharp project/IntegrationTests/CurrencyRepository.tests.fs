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
    let aCurrency:Currency = { Code=TEST_CODE; Name="Aaaaa"}

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
        repository.Create(aCurrency)
        let storedCurrency = repository.Single(aCurrency.Code)
        storedCurrency |> should equal (Some aCurrency)

    [<Test>]
    member this.``Single [when] item does not exists [should] return None`` () =
        repository.Single(aCurrency.Code) |> should equal None

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

    // TODO: test Single with bof success and fail
