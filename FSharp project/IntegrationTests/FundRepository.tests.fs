namespace IntegrationTests.Repository

open System
open NUnit.Framework
open FsUnit
open MongoDB.Driver

open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open NUnit.Framework.Constraints
open helper


type equalFundAtDate(expected:FundAtDate) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? option<FundAtDate> as fund ->
            fund.IsSome |> should be True
            fund.Value.Id |> should equal expected.Id
            fund.Value.Date |> should equalDate expected.Date
            fund.Value.CurrencyCode |> should equal expected.CurrencyCode
            fund.Value.FundCompanyId |> should equal expected.FundCompanyId
            fund.Value.Quantity |> should equal expected.Quantity
            ConstraintResult(this, actual, true)
        | _ ->
            ConstraintResult(this, actual, false)

type ``Fund Repository`` () =

    let repository = FundRepository(configuration.connectionString, "Fund_test") :> IFundRepository
    let TEST_ID = "TEST 1"
    let TEST_CURRENCY = "TESTTEST 1"
    let TEST_CURRENCY_2 = "TESTTEST 2"
    // no millisecond fraction because of limited precision on repository precision
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
    member this.``GetFundsToDate [should] return a fund saved in previous date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(-10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; LastChangeDate=DateTime.UtcNow } 
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
    member this.``GetFundsToDate [should] NOT return a fund saved in successive date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(+10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; LastChangeDate=Now } 
        addRecord(item)

        // execute
        let data = repository.GetFundsToDate(toDate)

        data |> should be Empty

    [<Test>]
    member this.``GetFundsToDate [should] return only expected records`` () =
        let toDate = DateTime(2000, 01, 31)
        let oldDate = toDate.AddDays(-20.);
        let beforeDate = toDate.AddDays(-10.);
        let afterDate = toDate.AddDays(+10.);
        // valid record
        let item_1:FundAtDate = {Id="1"; Date=toDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now}
        let item_2:FundAtDate = {Id="2"; Date=beforeDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now } 
        // non valid
        let item_3:FundAtDate = {Id="3"; Date=beforeDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now }
        let item_4:FundAtDate = {Id="4"; Date=oldDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now }
        let item_5:FundAtDate = {Id="5"; Date=afterDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now} 
        let item_6:FundAtDate = {Id="6"; Date=oldDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=Now } 
        addRecords([item_1; item_2; item_3; item_4; item_5; item_6])

        // execute
        let data = repository.GetFundsToDate(toDate)

        data.Length |> should equal 2
        data |> should contain item_1
        data |> should contain item_2


    [<Test>]
    member this.``FindFundAtDate [should] return the record`` () =
        let fundAtDate:FundAtDate = { Id="123"; Date=DateTime.Today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=Now}
        addRecord fundAtDate

        let t1 = fundAtDate.LastChangeDate
        let t2 = (repository.FindFundAtDate fundAtDate).Value.LastChangeDate

        let ticks1 = t1.Ticks
        let ticks2 = t2.Ticks
        // execute
        repository.FindFundAtDate fundAtDate
        |> should equal (Some fundAtDate)

    [<Test>]
    member this.``FindFundAtDate [when] differ by date [should] not return the record`` () =

        let fundAtDate:FundAtDate = { Id="123"; Date=DateTime.Today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        addRecord fundAtDate

        let newRecord = { fundAtDate with Date = fundAtDate.Date.AddDays(10.); Quantity = 2m}

        // execute
        repository.FindFundAtDate newRecord
        |> should equal None


    [<Test>]
    member this.``FindFundAtDate [when] record does not exist [should] return None`` () =

        let fundAtDate:FundAtDate = { Id=""; Date=DateTime.Today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        // execute
        repository.FindFundAtDate fundAtDate
        |> should equal None


    [<Test>]
    member this.``CreateFundAtDate`` () =

        let fundAtDate:FundAtDate = { Id=TEST_ID; Date=DateTime.Today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        // execute
        repository.CreateFundAtDate fundAtDate

        let savedRecord = repository.FindFundAtDate(fundAtDate)
        savedRecord |> should equalFundAtDate fundAtDate


    [<Test>]
    member this.``UpdateFundAtDate`` () =

        let fundAtDate:FundAtDate = { 
            Id=TEST_ID; Date=DateTime.Today
            CurrencyCode=TEST_CURRENCY
            FundCompanyId="Company"; Quantity=1m
            LastChangeDate=DateTime.UtcNow }
        repository.CreateFundAtDate fundAtDate

        let updateAtDate:FundAtDate = { 
            Id=TEST_ID; Date=DateTime.Today.AddDays(2.)
            CurrencyCode = TEST_CURRENCY_2
            FundCompanyId = "Company 2"
            Quantity = 2m
            LastChangeDate=DateTime.UtcNow }

        // execute
        repository.UpdateFundAtDate updateAtDate

        let savedRecord = repository.FindFundAtDate(updateAtDate)
        savedRecord |> should equalFundAtDate updateAtDate