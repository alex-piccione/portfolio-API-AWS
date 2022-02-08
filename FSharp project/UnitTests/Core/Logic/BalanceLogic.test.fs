namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities

type BalanceLogicTest() =

    let Now = DateTime.UtcNow
    let chronos = Mock<IChronos>().SetupPropertyGet(fun c -> c.Now).Returns(Now).Create()
    let idGenerator = Mock<IIdGenerator>().Create()

    let fundAtDate: FundAtDate = {
        Id = Guid.NewGuid().ToString()
        Date = DateTime.Today
        CurrencyCode = "AAA"
        FundCompanyId = "Company"
        Quantity = 1m
        LastChangeDate = Now.AddMonths(-1)
    }

    let mockRepository = Mock<IFundRepository>().Create()

    [<SetUp>]
    member this.SetUp() =
        ()

    [<Test>]
    member this.``GetBalance with a simple scenario``() =

        let date = DateTime(2010, 08, 15)
        let older_date = DateTime(2010, 07, 15)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m; LastChangeDate = date}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m; LastChangeDate = older_date}
        ]

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]; LastUpdateDate=date}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 1.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency
        balance.LastUpdateDate |> should equal date

    [<Test>]
    member this.``GetBalance with a complex scenario``() =

        let date = DateTime(2010, 08, 15)
        //let old_date = date.AddDays(-10.)
        let older_date = date.AddDays(-100.)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m; LastChangeDate = Now}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m; LastChangeDate = Now}
            {Id="4"; Date=date; CurrencyCode="BBB"; FundCompanyId="Company A"; Quantity=4m; LastChangeDate = Now}
        ]

        let expectedFundForCurrency_AAA:FundForCurrency = { CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]; LastUpdateDate=Now}
        let expectedFundForCurrency_BBB:FundForCurrency = { CurrencyCode="BBB"; Quantity=4m; CompaniesIds=["Company A"]; LastUpdateDate=Now}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 2.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency_AAA
        balance.FundsByCurrency.[1] |> should equal expectedFundForCurrency_BBB
        balance.LastUpdateDate |> should equal Now

    [<Test>]
    member this.``GetBalance [when] latest quantity is zero [should] not take the record``() =

        let date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=0m; LastChangeDate = Now}
        ]

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be True

    [<Test>]
    member this.``GetBalance [when] latest quantity on a Company is zero [should] not take the company``() =

        let date = DateTime(2010, 08, 15)
        let older_date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="1"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=1m; LastChangeDate = Now}
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=0m; LastChangeDate = Now}
        ]

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=1m; CompaniesIds=["Company B"]; LastUpdateDate=Now}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 1.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency

    [<Test>]
    member this.``GetBalance [should] return the LastUpdateDate``() =

        let date = DateTime(2010, 08, 15)
        let changeDate1 = DateTime(2020, 06, 01)
        let changeDate2 = DateTime(2020, 12, 01) // newer
        let changeDate3 = DateTime(2020, 08, 01)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company"; Quantity=0m; LastChangeDate = changeDate1}
            {Id="2"; Date=date; CurrencyCode="BBB"; FundCompanyId="Company"; Quantity=0m; LastChangeDate = changeDate2}
            {Id="3"; Date=date; CurrencyCode="CCC"; FundCompanyId="Company"; Quantity=0m; LastChangeDate = changeDate3}
        ]

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        (logic.GetBalance date).LastUpdateDate |> should equal changeDate2

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
        logic.CreateOrUpdate update |> should equal Updated

        let isExpectedRecord = 
            fun r -> 
                r.Id |> should equal (expectedRecord.Id)
                r.Date |> should equal (expectedRecord.Date)
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
            Date = fundAtDate.Date.AddDays(1.) // a different date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord = fun r -> r.Id |> should equal "new id"
                                      r.Date |> should equal request.Date
                                      r.CurrencyCode |> should equal request.CurrencyCode
                                      r.FundCompanyId |> should equal request.CompanyId 
                                      r.Quantity |> should equal request.Quantity 
                                      r.LastChangeDate |> should equal Now
                                      true
        // execute
        logic.CreateOrUpdate request |> should equal Created
        verify <@ fundRepository.CreateFundAtDate (is expectedRecord) @> once

    [<Test>]
    member this.``CreateOrUpdate [when] date contains time part [should] match record within the same day`` () =
        let fundRepository = Mock<IFundRepository>()
                                .Setup( fun r -> r.FindFundAtDate (any()) )
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = DateTime.UtcNow
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord = fun r -> 
                                 r.Id |> should equal fundAtDate.Id
                                 r.Date |> should equal request.Date.Date
                                 r.Quantity |> should equal request.Quantity
                                 r.LastChangeDate |> should equal Now
                                 true
        // execute
        logic.CreateOrUpdate request |> should equal Updated
        verify <@ fundRepository.UpdateFundAtDate (is expectedRecord) @> once

    [<Test>]
    member this.``Update [when] Date is missing [should] return error``() =
        let request:BalanceUpdateRequest = {
            Date = DateTime.MinValue
            CurrencyCode = ""
            Quantity = 1m
            CompanyId = "company"
        }

        match (BalanceLogic(mockRepository, chronos, idGenerator) :> IBalanceLogic).CreateOrUpdate(request) with
        | InvalidRequest error -> error |> should equal (error_messages.mustBeDefined "Date")
        | x -> x |> failwith $"unexpected result: {x} instead of {InvalidRequest}"

    [<Test>]
    member this.``Update [when] Quantity is not positive [should] return error``() =
        let request:BalanceUpdateRequest = {
            Date = DateTime.UtcNow
            CurrencyCode = "AAA"
            Quantity = 0m
            CompanyId = "company"
        }

        match (BalanceLogic(mockRepository, chronos, idGenerator) :> IBalanceLogic).CreateOrUpdate(request) with
        | InvalidRequest error -> error |> should equal (error_messages.mustBeGreaterThanZero "Quantity")
        | x -> x |> failwith $"unexpected result: {x} instead of {InvalidRequest}"


    [<Test>]
    member this.``Update [when] Company is missing [should] return error``() =
        let request:BalanceUpdateRequest = {
            Date = DateTime.UtcNow
            CurrencyCode = ""
            Quantity = 1m
            CompanyId = ""
        }

        match (BalanceLogic(mockRepository, chronos, idGenerator) :> IBalanceLogic).CreateOrUpdate(request) with
        | InvalidRequest error -> error |> should equal (error_messages.mustBeDefined "CompanyId")
        | x -> x |> failwith $"unexpected result: {x} instead of {InvalidRequest}"