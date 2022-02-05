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

    let fundAtDate: FundAtDate = {
        Id = Guid.NewGuid().ToString()
        Date = DateTime.Today
        CurrencyCode = "AAA"
        FundCompanyId = "Company"
        Quantity = 1m
        LastChangeDate = Now.AddMonths(-1)
    }

    [<SetUp>]
    member this.SetUp() =
        ()

    [<Test>]
    member this.``GetBalance with a simple case``() =

        let date = DateTime(2010, 08, 15)
        let older_date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m; LastChangeDate = Now}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m; LastChangeDate = Now}
        ]

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 1.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency

    [<Test>]
    member this.``GetBalance with a complex case``() =

        let date = DateTime(2010, 08, 15)
        let old_date = date.AddDays(-10.)
        let older_date = date.AddDays(-100.)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m; LastChangeDate = Now}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m; LastChangeDate = Now}
            {Id="4"; Date=date; CurrencyCode="BBB"; FundCompanyId="Company A"; Quantity=4m; LastChangeDate = Now}
        ]

        let expectedFundForCurrency_AAA:FundForCurrency = { CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]}
        let expectedFundForCurrency_BBB:FundForCurrency = { CurrencyCode="BBB"; Quantity=4m; CompaniesIds=["Company A"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 2.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency_AAA
        balance.FundsByCurrency.[1] |> should equal expectedFundForCurrency_BBB

    [<Test>]
    member this.``GetBalance [when] latest quantity is zero [should] not take the record``() =

        let date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=0m; LastChangeDate = Now}
        ]

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

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

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=1m; CompaniesIds=["Company B"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 1.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency

    [<Test>]
    member this.``Update [when] record exists [should] update`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.FindFundAtDate(any()))
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

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
        logic.Update update |> should equal Updated

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
    member this.``Update [when] record does not exist [should] create`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup( fun r -> r.FindFundAtDate(any()) )
                                .Returns(None)
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = fundAtDate.Date.AddDays(1.) // a different date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord = fun r -> r.Id |> should not' (be Empty)
                                      r.Id |> should not' (equal fundAtDate.Id)
                                      r.Date |> should equal request.Date
                                      r.CurrencyCode |> should equal request.CurrencyCode
                                      r.FundCompanyId |> should equal request.CompanyId 
                                      r.Quantity |> should equal request.Quantity 
                                      r.LastChangeDate |> should equal Now
                                      true
        // execute
        logic.Update request |> should equal Created
        verify <@ fundRepository.CreateFundAtDate (is expectedRecord) @> once

    [<Test>]
    member this.``Update [when] date contains time part [should] match record within same day`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup( fun r -> r.FindFundAtDate (any()) )
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos) :> IBalanceLogic

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
        logic.Update request |> should equal Updated
        verify <@ fundRepository.UpdateFundAtDate (is expectedRecord) @> once
