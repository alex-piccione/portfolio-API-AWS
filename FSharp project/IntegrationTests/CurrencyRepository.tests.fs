namespace IntegrationTests.Repository

open NUnit.Framework
open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open FsUnit
open NUnit.Framework.Constraints

type equalCurrency(expected:Currency) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? option<Currency> as co ->
            co.IsSome |> should be True
            co.Value.Code |> should equal expected.Code
            co.Value.Name |> should equal expected.Name
            ConstraintResult(this, actual, true)
        | _ ->
            ConstraintResult(this, actual, false)

type ``Currency Repository`` () =

    let repository = CurrencyRepository(configuration.connectionString, "Currency_test") :> ICurrencyRepository
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
    member this.``All`` () =
        repository.Create({ aCurrency with Code=TEST_CODE })
        repository.Create({ aCurrency with Code=TEST_CODE_2 })

        // execute
        let items = repository.All() |> List.ofSeq
        items |> should contain { aCurrency with Code=TEST_CODE}
        items |> should contain { aCurrency with Code=TEST_CODE_2}

    [<Test>]
    member this.``Update`` () =
        repository.Create(aCurrency)
        let itemToUpdate = { aCurrency with Name="New Name" }

        // execute
        repository.Update(itemToUpdate)

        let unpdatedItem = repository.Single(itemToUpdate.Code)
        unpdatedItem |> should equalCurrency itemToUpdate

    [<Test>]
    member this.``Delete`` () =
        repository.Create(aCurrency)

        // execute
        repository.Delete(aCurrency.Code)

        repository.Single(aCurrency.Code).IsNone |> should be True 

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
        repository.ExistsWithName("Currency aaa") |> should be True
        repository.ExistsWithName("Currency BBB") |> should be False