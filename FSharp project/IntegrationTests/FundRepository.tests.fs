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
open configuration


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

// TODO: implement and use it. Currently not able to call.
type containItem(find:Func<FundAtDate, bool>) = 
    inherit Constraints.EqualConstraint(find)

    override this.Description = "should contain zzz"

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? option<FundAtDate> as fund ->
            fund.IsSome |> should be True            
            let result = find.Invoke(fund.Value)    
            ConstraintResult(this, actual, result)
        | _ ->
            ConstraintResult(this, actual, false)

type containItemWithId(id:string) = 
    inherit Constraints.EqualConstraint(id)

    override this.ApplyTo<'C> (actual: 'C):  ConstraintResult =
        match box actual with 
        | :? (FundAtDate list) as funds ->
            let result = funds |> List.exists (fun x -> x.Id = id)
            ConstraintResult(this, actual, result)
        | _ ->
            ConstraintResult(this, actual, false)

type ``Fund Repository`` () =
    let repository = FundRepository(databaseConfig, "Fund") :> IFundRepository
    let TEST_ID = "TEST 1"
    let TEST_CURRENCY = "TESTTEST 1"
    let TEST_CURRENCY_2 = "TESTTEST 2"
    // no millisecond fraction because of limited precision on repository precision
    let now = DateTime(2022, 12, 31, 01, 02, 03, 999, DateTimeKind.Utc) 
    let today = DateTime.UtcNow.Date
    
    let client = new MongoClient(MongoClientSettings.FromConnectionString(databaseConfig.ConnectionString))
    let collection = client.GetDatabase(databaseConfig.Database).GetCollection("Fund")

    let newGuid() = Guid.NewGuid().ToString()
    let addRecord item = collection.InsertOne item
    let addRecords items = items |> List.iter collection.InsertOne

    let item:FundAtDate = {Id=newGuid(); Date=today; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; LastChangeDate=now } 

    [<SetUp>]
    member this.Setup () =
        collection.DeleteMany (FilterDefinitionBuilder<FundAtDate>().Empty) |> ignore
 
    [<Test>]
    member this.``GetFundsToDate [should] return a fund saved in previous date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(-10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; LastChangeDate=now } 
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
    member this.``GetFundsToDate [should] NOT return a fund saved on after the reference date`` () =
        let toDate = DateTime(2000, 01, 31)
        let itemDate = toDate.AddDays(+10.);
        let item:FundAtDate = {Id=newGuid(); Date=itemDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=123.456789m; LastChangeDate=now } 
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
        let item_1:FundAtDate = {Id="1"; Date=toDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now}
        let item_2:FundAtDate = {Id="2"; Date=beforeDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now } 
        let item_3:FundAtDate = {Id="3"; Date=beforeDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="GGG"; Quantity=1.1m; LastChangeDate=now}
        // non valid
        let item_10:FundAtDate = {Id="10"; Date=beforeDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now }
        let item_11:FundAtDate = {Id="11"; Date=oldDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now }
        let item_12:FundAtDate = {Id="12"; Date=afterDate; CurrencyCode=TEST_CURRENCY; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now} 
        let item_13:FundAtDate = {Id="13"; Date=oldDate; CurrencyCode=TEST_CURRENCY_2; FundCompanyId="FFF"; Quantity=1m; LastChangeDate=now } 
        addRecords([
            item_1; item_2; item_3; 
            item_10; item_11; item_12; item_13])

        // execute
        let data = repository.GetFundsToDate(toDate)

        data.Length |> should equal 3
        data |> should contain item_1
        data |> should contain item_2
        data |> should contain item_3


    [<Test>]
    member this.``FindFundAtDate [should] return the record`` () = 
        let fundAtDate:FundAtDate = { Id="123"; Date=today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=now}
        let fundAtDate_2 = { fundAtDate with Id="124"; Date=today.AddDays(+1); CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=100m; LastChangeDate=now}
        let fundAtDate_3 = { fundAtDate with Id="125"; Date=today.AddDays(-1); CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=100m; LastChangeDate=now}
        addRecord fundAtDate
        addRecord fundAtDate_2
        addRecord fundAtDate_3

        // execute
        repository.FindFundAtDate fundAtDate |> should equal (Some fundAtDate)

    [<Test>]
    member this.``FindFundAtDate [when] differ by date [should] not return the record`` () =
        let fundAtDate:FundAtDate = { Id="123"; Date=today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        addRecord fundAtDate

        let newRecord = { fundAtDate with Date = today.AddDays(1.) }

        // execute
        repository.FindFundAtDate newRecord |> should equal None


    [<Test>]
    member this.``FindFundAtDate [when] record does not exist [should] return None`` () =
        let fundAtDate:FundAtDate = { Id=""; Date=today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        // execute
        repository.FindFundAtDate fundAtDate |> should equal None


    [<Test>]
    member this.``CreateFundAtDate`` () =        
        let fundAtDate:FundAtDate = { Id=TEST_ID; Date=today; CurrencyCode=TEST_CURRENCY; FundCompanyId="Company"; Quantity=1m; LastChangeDate=DateTime.UtcNow}

        // execute
        repository.CreateFundAtDate fundAtDate

        let savedRecord = repository.FindFundAtDate(fundAtDate)
        savedRecord |> should equalFundAtDate fundAtDate


    [<Test>]
    member this.``UpdateFundAtDate`` () =

        let fundAtDate:FundAtDate = { 
            Id=TEST_ID; Date=today
            CurrencyCode=TEST_CURRENCY
            FundCompanyId="Company"; Quantity=1m
            LastChangeDate=DateTime.UtcNow }
        repository.CreateFundAtDate fundAtDate

        let updateAtDate:FundAtDate = { 
            Id=TEST_ID; Date=today.AddDays(-2.)
            CurrencyCode = TEST_CURRENCY_2
            FundCompanyId = "Company 2"
            Quantity = 2m
            LastChangeDate=DateTime.UtcNow }

        // execute
        repository.UpdateFundAtDate updateAtDate

        let savedRecord = repository.FindFundAtDate(updateAtDate)
        savedRecord |> should equalFundAtDate updateAtDate

    //[<Test>]
    //member this.``GetFundsOfCompany [should] return funds of that company`` () =
    //    let companyId = "aaa"
    //    let data = repository.GetFundsOfCompany companyId

    [<Test>]
    member this.``GetFundsOfCurrency [should] return funds of that currency with Date equal or after the limit`` () =
        let currencyCode = "aaa"
        let aDay = DateTime(2000, 1, 1)

        addRecords([
            {item with Id="1"; CurrencyCode="bbb"; Date=aDay} // no, wrong currency
            {item with Id="2"; CurrencyCode="aaa"; Date=aDay} // ok
            {item with Id="3"; CurrencyCode="aaa"; Date=aDay.AddDays(-1)} // no, earlier than limit           
            {item with Id="4"; CurrencyCode="aaa"; Date=aDay.AddDays(1)} // ok
        ])

        // execute
        let data = repository.GetFundsOfCurrency(currencyCode, aDay)
        data |> should haveLength 2
        // data |> should containItem (fun x -> x.Id == "1")
        data |> should containItemWithId "2"
        data |> should containItemWithId "4"