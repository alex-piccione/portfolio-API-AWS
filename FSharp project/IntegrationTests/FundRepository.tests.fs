namespace IntegrationTests.Repository

open System
open NUnit.Framework
open FsUnit
open MongoDB.Driver

open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities


type ``Fund Repository`` () =

    let repository = FundRepository(configuration.connectionString) :> IFundRepository
    let TEST_CURRENCY = "TESTTEST 1"
    let TEST_CURRENCY_2 = "TESTTEST 2"

    //let delete id = repository.Delete id#
    let client = new MongoClient(MongoClientSettings.FromConnectionString(configuration.connectionString))
    let collection = client.GetDatabase("Portfolio").GetCollection("Fund")

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
 
    [<TearDown>]
    member this.TearDown () =
        ()
 
    [<Test>]
    member this.``GetFundsToDate <should> return a fund saved in previous date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(-10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m } 
        addRecord(item)

        // execute
        let data = repository.GetFundsToDate(toDate)

        data |> should not' (be Empty)
        data.IsEmpty  |> should be False
        let fund = data.[0]
        fund.Date |> should equal (item.Date)
        fund.CurrencyCode |> should equal (item.CurrencyCode)
        fund.FundCompanyId |> should equal (item.FundCompanyId)
        fund.Quantity |> should equal (item.Quantity)


    [<Test>]
    member this.``GetFundsToDate <should> NOT return a fund saved in successive date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(+10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m } 
        addRecord(item)

        // execute
        let data = repository.GetFundsToDate(toDate)

        data |> should be Empty

    [<Test>]
    member this.``GetFundsToDate <should> return only expected records`` () =
        let toDate = DateTime(2000, 01, 31)
        let oldDate = toDate.AddDays(-20.);
        let beforeDate = toDate.AddDays(-10.);
        let afterDate = toDate.AddDays(+10.);
        // valid record
        let item_1:FundAtDate = {Id="1"; Date=toDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m }
        let item_2:FundAtDate = {Id="2"; Date=beforeDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m } 
        // non valid
        let item_3:FundAtDate = {Id="3"; Date=beforeDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m }
        let item_4:FundAtDate = {Id="4"; Date=oldDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m }
        let item_5:FundAtDate = {Id="5"; Date=afterDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m } 
        let item_6:FundAtDate = {Id="6"; Date=oldDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m } 
        addRecords([item_1; item_2; item_3; item_4; item_5; item_6])

        // execute
        let data = repository.GetFundsToDate(toDate)

        data.Length |> should equal 2
        data |> should contain item_1
        data |> should contain item_2
