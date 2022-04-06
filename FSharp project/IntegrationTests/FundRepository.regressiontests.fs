namespace IntegrationTests.Repository

open System
open NUnit.Framework
open FsUnit
open MongoDB.Driver

open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open helper


type FundAtDateWithoutLastChangeDate = {
     Id: string
     Date: DateTime
     CurrencyCode: string
     FundCompanyId: string
     Quantity: decimal
}  

type ``Fund Repository (regression)`` () =

    let repository = FundRepository(configuration.connectionString, "Fund_test") :> IFundRepository
    let TEST_ID = "TEST 1"
    let TEST_CURRENCY = "TESTTEST 1"
    let TEST_CURRENCY_2 = "TESTTEST 2"

    // no millisecond fraction because of limited precision on repository
    let Now = DateTime(2022, 12, 31, 01, 02, 03, 999, DateTimeKind.Utc) 

    let client = new MongoClient(MongoClientSettings.FromConnectionString(configuration.connectionString))
    let collection = client.GetDatabase("Portfolio").GetCollection("Fund_test")

    let newGuid() = Guid.NewGuid().ToString()

    let addRecord item = collection.InsertOne item

    let addRecords items = items |> List.iter collection.InsertOne

    let removeAllRecords item = 
        let builder = FilterDefinitionBuilder<FundAtDate>()
        let filter = builder.Or([
            builder.Eq( (fun i -> i.CurrencyCode), TEST_CURRENCY)
            builder.Eq( (fun i -> i.CurrencyCode), TEST_CURRENCY_2)
        ])
        collection.DeleteMany filter |> ignore

    [<SetUp>]
    member this.Setup () =
        removeAllRecords()
 
    [<Test>]
    member this.``GetFundsToDate [should] return a fund saved without LastChangeDate`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(-10.);
        let item:FundAtDateWithoutLastChangeDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; } 
        
        client.GetDatabase("Portfolio").GetCollection<FundAtDateWithoutLastChangeDate>("Fund_test")
            .InsertOne(item)

        // execute
        let data = repository.GetFundsToDate(toDate)

        data |> should not' (be Empty)
        data.IsEmpty  |> should be False
        let fund = data.[0]
        fund.Date |> should equal (item.Date)
        fund.CurrencyCode |> should equal (item.CurrencyCode)
        fund.FundCompanyId |> should equal (item.FundCompanyId)
        fund.Quantity |> should equal (item.Quantity)

