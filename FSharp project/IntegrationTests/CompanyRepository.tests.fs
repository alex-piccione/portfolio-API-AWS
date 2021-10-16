namespace IntegrationTests.Repository

open NUnit.Framework
open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open FsUnit
open NUnit.Framework.Constraints


type equalCompany(expected:Company) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? option<Company> as co ->
            co.IsSome |> should be True
            co.Value.Id |> should equal expected.Id
            co.Value.Name |> should equal expected.Name
            co.Value.Types |> should equivalent expected.Types
            ConstraintResult(this, actual, true)
        | _ ->
            ConstraintResult(this, actual, false)


type ``Company Repository`` () =

    let repository = CompanyRepository(configuration.connectionString) :> ICompanyRepository
    let TEST_ID = "TEST 1"
    let TEST_ID_2 = "TEST 2"

    let delete id = repository.Delete id

    [<SetUp>]
    member this.Setup () =
        delete TEST_ID
        delete TEST_ID_2
 
    [<TearDown>]
    member this.TearDown () =
        delete TEST_ID
        delete TEST_ID_2
 
    [<Test>]
    member this.``Create & Read`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types= [CompanyType.Bank] }
        repository.Create(item)
        let storedItem = repository.Single(TEST_ID)
        storedItem |> should equal (Some item)

    [<Test>]
    member this.``Create & Read <when> multiple company types`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types=[CompanyType.Stacking; CompanyType.Bank] }
        repository.Create(item)
        let storedItem = repository.Single(TEST_ID)
        storedItem |> should equalCompany item

    [<Test>]
    member this.``Update`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types= [CompanyType.Bank] }
        repository.Create(item)

        let itemToUpdate = {item with Name= "Company Update"; Types=[CompanyType.Stacking]}

        // execute
        repository.Update(itemToUpdate)

        let unpdatedItem = repository.Single(TEST_ID)
        unpdatedItem |> should equalCompany itemToUpdate

    [<Test>]
    member this.``Delete`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types= [CompanyType.Bank] }
        repository.Create(item)

        // execute
        repository.Delete(item.Id)

        repository.Single(TEST_ID).IsNone |> should be True 

    [<Test>]
    member this.``Exists`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types= [CompanyType.Bank] }
        repository.Create(item)

        // execute
        let result = repository.Exists("company test")

        result |> should be True 

    [<Test>]
    member this.``GetByName`` () =
        let item:Company = { Id=TEST_ID; Name="Company Test"; Types= [CompanyType.Bank] }
        repository.Create(item)

        // execute
        let result = repository.GetByName("company test")

        result |> should equal (Some item)