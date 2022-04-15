namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open NUnit.Framework.Constraints
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities

type equalResult(expected:Result<_,_>) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual:'C):  ConstraintResult =   
        match box actual with 
        | :? Result<BalanceUpdateResult,string> as result -> 
            match expected with 
            | Ok x -> 
               match result with 
               | Ok y -> 
                    x |> should equal y
                    ConstraintResult(this, actual, true)
               | _ -> ConstraintResult(this, actual, false)
            | Error e -> 
                match result with 
                | Error f -> 
                    f |> should equal (unbox e)
                    ConstraintResult(this, actual, true)
                | _ -> ConstraintResult(this, actual, false) 
        | :? Result<obj,obj> as result -> ConstraintResult(this, actual, true)
        | _ -> ConstraintResult(this, actual, false)  
            
type BalanceLogicTest() =
    let Now = DateTime.UtcNow
    let chronos = Mock<IChronos>().SetupPropertyGet(fun c -> c.Now).Returns(Now).Create()
    let idGenerator = Mock<IIdGenerator>().Create()

    let fundAtDate: FundAtDate = {
        Id = Guid.NewGuid().ToString()
        Date = Now.AddMonths(-1)
        CurrencyCode = "AAA"
        FundCompanyId = "Company"
        Quantity = 1m
        LastChangeDate = Now.AddDays(-1)
    }

    let request:BalanceUpdateRequest = {
        Date = Now.AddDays(-1)
        CurrencyCode = "EUR"
        Quantity = 1m
        CompanyId = "company"
    }

    let mockRepository = Mock<IFundRepository>().Create()

    let testRequestValidation request expectedError = 
        let error method = Exception($"call to \"{method}\" not expected")
        let mockRepository = 
            Mock<IFundRepository>()
                .Setup(fun r -> r.FindFundAtDate (any()) ).Raises(error "FindFundAtDate") 
                .Setup(fun r -> r.CreateFundAtDate (any()) ).Raises(error "CreateFundAtDate") 
                .Setup(fun r -> r.UpdateFundAtDate (any()) ).Raises(error "UpdateFundAtDate") 
                .Create()

        match (BalanceLogic(mockRepository, chronos, idGenerator) :> IBalanceLogic).CreateOrUpdate(request) with
        | Error error -> error |> should equal expectedError
        | x -> x |> failwith $"unexpected result: {x} instead of {Error}"

    [<SetUp>]
    member this.SetUp() =
        ()

    [<Test>]
    member this.``CreateOrUpdate [when] record exists [should] update`` () =
        let fundRepository = Mock<IFundRepository>()
                                 .Setup(fun r -> r.FindFundAtDate (any()))
                                 .Returns(Some(fundAtDate))
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        let update:BalanceUpdateRequest = {
            Date = fundAtDate.Date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord:FundAtDate = { 
            fundAtDate with 
                Id = fundAtDate.Id; 
                Quantity = update.Quantity 
                LastChangeDate = Now
        }

        // execute
        logic.CreateOrUpdate update |> should equalResult (Ok Updated)

        let isExpectedRecord = 
            fun (r:FundAtDate) -> 
                r.Id |> should equal (expectedRecord.Id)
                r.Date |> should equal (expectedRecord.Date.Date)
                r.CurrencyCode |> should equal (expectedRecord.CurrencyCode)
                r.FundCompanyId |> should equal (expectedRecord.FundCompanyId)
                r.Quantity |> should equal (expectedRecord.Quantity)
                r.LastChangeDate |> should equal Now
                true

        verify <@ fundRepository.UpdateFundAtDate (is isExpectedRecord) @> once


    [<Test>]
    member this.``CreateOrUpdate [when] record does not exist [should] create a new one`` () =
        let fundRepository = Mock<IFundRepository>()
                                 .Setup(fun r -> r.FindFundAtDate (any()))
                                 .Returns(None)
                                 .Create()
        let idGenerator = Mock<IIdGenerator>().Setup(fun g -> g.New()).Returns("new id").Create()

        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = fundAtDate.Date.AddDays(-1) // a different date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        // execute
        logic.CreateOrUpdate request |> should equalResult (Ok Created)

        let expectedRecord = 
            fun (r:FundAtDate) -> 
                r.Id |> should equal "new id"
                r.Date |> should equal request.Date.Date
                r.CurrencyCode |> should equal request.CurrencyCode
                r.FundCompanyId |> should equal request.CompanyId 
                r.Quantity |> should equal request.Quantity 
                r.LastChangeDate |> should equal Now
                true
                
        let expectedFundAtDate = fun (f:FundAtDate) -> f.CurrencyCode |> should equal request.CurrencyCode
                                                       f.Date.Date |> should equal request.Date.Date
                                                       true

        verify <@ fundRepository.FindFundAtDate (is expectedFundAtDate) @> once
        verify <@ fundRepository.CreateFundAtDate (is expectedRecord) @> once

    [<Test>]
    member this.``CreateOrUpdate [when] same day but different time [should] update record within the same day`` () =
        let fundRepository = Mock<IFundRepository>()
                                .Setup( fun r -> r.FindFundAtDate (any()) )
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            request with
                Date = fundAtDate.Date.AddHours(5) // same day
                CurrencyCode = fundAtDate.CurrencyCode
                CompanyId = fundAtDate.FundCompanyId
                Quantity = 2m
        }

        let expectedRecord = 
            fun (r:FundAtDate) -> 
                r.Id |> should equal fundAtDate.Id
                r.Date |> should equal request.Date.Date
                r.Quantity |> should equal request.Quantity
                r.LastChangeDate |> should equal Now
                true

        // execute
        logic.CreateOrUpdate request |> should equalResult (Ok Updated)
        verify <@ fundRepository.UpdateFundAtDate (is expectedRecord) @> once

    [<Test>]
    member this.``CreateOrUpdate [when] Date is missing [should] return error``() =
        let request = {request with Date = DateTime.MinValue}
        let expectedError = error_messages.mustBeDefined "Date"
        testRequestValidation request expectedError

    [<Test>]
    member this.``CreateOrUpdate [when] Date is in the future [should] return error``() =
        testRequestValidation 
            {request with Date = DateTime.UtcNow.AddDays(1)}
            (error_messages.mustBeInThePast "Date")

    [<Test>]
    member this.``CreateOrUpdate [when] CurrencyCode is missing [should] return error``() =
        testRequestValidation
            {request with CurrencyCode = ""}
            (error_messages.mustBeDefined "CurrencyCode")

    [<Test>]
    member this.``CreateOrUpdate [when] Quantity is not positive [should] return error``() =
        testRequestValidation
            {request with Quantity = 0m}
            (error_messages.mustBeGreaterThanZero "Quantity")

    [<Test>]
    member this.``CreateOrUpdate [when] Company is missing [should] return error``() =
        testRequestValidation
            {request with CompanyId = ""}
            (error_messages.mustBeDefined "CompanyId")

    [<Test>]
    member this.``GetFundOfCurrencyByDate [should] return proper result (simple case)``() =
        let currencyCode = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let date1 = new DateTime(2000, 01, 01)
        let date2 = new DateTime(2000, 02, 01)
        let updateDate = new DateTime(2000, 04, 05)
        let company1 = "C1"
        let company2 = "C2"

        let fundAtDate:FundAtDate = { Id=""; Date=date1; CurrencyCode=currencyCode; FundCompanyId="c1"; Quantity=1m; LastChangeDate=updateDate }

        // AAA 
        //   c1: d1 d2  -> d1, d2
        // BBB
        //   c1: d1 -- -- --  -> --
        let funds = [
            {fundAtDate with Id="1"; Date=date1; FundCompanyId=company1; Quantity=1m}
            {fundAtDate with Id="2"; Date=date2; FundCompanyId=company2; Quantity=2m}
            {fundAtDate with Id="3"; Date=date2; FundCompanyId=company1; Quantity=4m}
        ]

        let record:CompanyFund = {Id=""; CompanyId=""; Quantity=0m; LastUpdateDate=updateDate}
        let expectedResult:CurrencyFundAtDate list = [
                {Date=date1; TotalQuantity=1m; CompanyFunds=[
                    {record with Id="1"; CompanyId=company1; Quantity=1m;}
                ]}
                {Date=date2; TotalQuantity=6m; CompanyFunds=[
                    {record with Id="2"; CompanyId=company2; Quantity=2m;}  
                    {record with Id="3"; CompanyId=company1; Quantity=4m;}                
                ]}
            ]
        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.GetFundsOfCurrency(currencyCode, minDate))
                                .Returns(funds)
                                .Create()
        // execute
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        logic.GetFundOfCurrencyByDate(currencyCode, minDate) |> should equal expectedResult
        verify <@ fundRepository.GetFundsOfCurrency(currencyCode, minDate) @> once

    [<Test>]
    member this.``GetFundOfCurrencyByDate [should] return proper result``() =
        let currencyCode = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let date1 = new DateTime(2000, 01, 01)
        let date2 = new DateTime(2000, 02, 01)
        let date3 = new DateTime(2000, 03, 01)
        let updateDate = new DateTime(2000, 04, 05)
        let company1 = "C1"
        let company2 = "C2"

        let fundAtDate:FundAtDate = { Id=""; Date=date1; CurrencyCode=currencyCode; FundCompanyId="c1"; Quantity=1m; LastChangeDate=updateDate }

        // AAA 
        //   c1: d1 d2 -- d4  -> d1, d2
        //   c2: -- d2 d3 --  ->     d2, d3
        let funds = [
            {fundAtDate with Id="1"; Date=date1; FundCompanyId=company1; Quantity=1m} 
            {fundAtDate with Id="3"; Date=date2; FundCompanyId=company1; Quantity=2m} 
            {fundAtDate with Id="4"; Date=date2; FundCompanyId=company2; Quantity=20m}
            {fundAtDate with Id="5"; Date=date3; FundCompanyId=company2; Quantity=30m} 
        ]

        let record:CompanyFund = {Id=""; CompanyId=""; Quantity=0m; LastUpdateDate=updateDate}
        let expectedResult:CurrencyFundAtDate list = [
                {Date=date1; TotalQuantity=1m; CompanyFunds=[
                    {record with Id="1"; CompanyId=company1; Quantity=1m;}
                ]}
                {Date=date2; TotalQuantity=22m; CompanyFunds=[
                    {record with Id="3"; CompanyId=company1; Quantity=2m;}
                    {record with Id="4"; CompanyId=company2; Quantity=20m;}                    
                ]}
                {Date=date3; TotalQuantity=30m; CompanyFunds=[
                    {record with Id="5"; CompanyId=company2; Quantity=30m;}
                ]}
            ]
        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.GetFundsOfCurrency(currencyCode, minDate))
                                .Returns(funds)
                                .Create()
        // execute
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        logic.GetFundOfCurrencyByDate(currencyCode, minDate) |> should equal expectedResult
        verify <@ fundRepository.GetFundsOfCurrency(currencyCode, minDate) @> once