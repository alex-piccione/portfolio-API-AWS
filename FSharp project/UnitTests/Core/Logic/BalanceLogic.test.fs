namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities
//


type BalanceLogicTest() =

    let fundAtDate: FundAtDate = {
        Id = Guid.NewGuid().ToString()
        Date = DateTime.Today
        CurrencyCode = "AAA"
        FundCompanyId = "Company"
        Quantity = 1m
    }

    [<SetUp>]
    member this.SetUp() =
        ()

    [<Test>]
    member this.``GetBalance with a simple case``() =

        let date = DateTime(2010, 08, 15)
        let older_date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m}
        ]

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

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
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=1m}
            {Id="2"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=2m}
            {Id="4"; Date=date; CurrencyCode="BBB"; FundCompanyId="Company A"; Quantity=4m}
        ]

        let expectedFundForCurrency_AAA:FundForCurrency = { CurrencyCode="AAA"; Quantity=3m; CompaniesIds=["Company A"; "Company B"]}
        let expectedFundForCurrency_BBB:FundForCurrency = { CurrencyCode="BBB"; Quantity=4m; CompaniesIds=["Company A"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

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
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=0m}
        ]

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be True

    [<Test>]
    member this.``GetBalance [when] latest quantity on a Company is zero [should] not take the company``() =

        let date = DateTime(2010, 08, 15)
        let older_date = DateTime(2010, 08, 15)
        let funds:FundAtDate list = [
            {Id="1"; Date=older_date; CurrencyCode="AAA"; FundCompanyId="Company B"; Quantity=1m}
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company A"; Quantity=0m}
        ]

        let expectedFundForCurrency:FundForCurrency = {CurrencyCode="AAA"; Quantity=1m; CompaniesIds=["Company B"]}

        let fundRepository = Mock<IFundRepository>()
                                 .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                 .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date
        balance.FundsByCurrency.IsEmpty |> should be False
        balance.FundsByCurrency.Length |> should equal 1.
        balance.FundsByCurrency.Head |> should equal expectedFundForCurrency

    [<Test>]
    member this.``Update [when] record exists [should] update`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup( (fun r -> r.FindFundAtDate(It.IsAny())) )
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = fundAtDate.Date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord:FundAtDate = { fundAtDate with Id = fundAtDate.Id; Quantity = request.Quantity }

        // execute
        logic.Update request |> should equal Updated
        verify <@ fundRepository.UpdateFundAtDate expectedRecord @> once

    [<Test>]
    member this.``Update [when] record does not exist [should] create`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup( (fun r -> r.FindFundAtDate(It.IsAny())) )
                                .Returns(None)
                                .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = fundAtDate.Date.AddDays(1.) // a different date
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedRecord:FundAtDate = { fundAtDate with Id = ""; Date = request.Date; Quantity = request.Quantity }

        // execute
        logic.Update request |> should equal Created
        verify <@ fundRepository.CreateFundAtDate expectedRecord @> once

    [<Test>]
    member this.``Update [when] date contains time part [should] match record within same day`` () =

        let fundRepository = Mock<IFundRepository>()
                                .Setup( (fun r -> r.FindFundAtDate(It.IsAny())) )
                                .Returns(Some(fundAtDate))
                                .Create()
        let logic = BalanceLogic(fundRepository) :> IBalanceLogic

        let request:BalanceUpdateRequest = {
            Date = DateTime.Now
            CurrencyCode = fundAtDate.CurrencyCode
            CompanyId = fundAtDate.FundCompanyId
            Quantity = 2m
        }

        let expectedDate = request.Date.Date
        let expectedRecord:FundAtDate = { fundAtDate with Id = fundAtDate.Id; Date = expectedDate; Quantity = request.Quantity }

        // execute
        logic.Update request |> should equal Updated
        verify <@ fundRepository.UpdateFundAtDate expectedRecord @> once