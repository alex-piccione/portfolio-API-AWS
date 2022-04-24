namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open FsUnit
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities
           
type ``BalanceLogic GetBalance Test``() =
    let Now = DateTime.UtcNow
    let chronos = Mock<IChronos>().SetupPropertyGet(fun c -> c.Now).Returns(Now).Create()
    let idGenerator = Mock<IIdGenerator>().Create()

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
    member this.``GetBalance [when] a Currency exists on multiple Companies [should] return the total at latest date``() =
        let date = DateTime(2020, 08, 15)
        let changeDate1 = DateTime(2020, 06, 01)
        let changeDate2 = DateTime(2020, 05, 01) 
        
        let funds:FundAtDate list = [
            {Id="1"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company 1"; Quantity=10m; LastChangeDate = changeDate1}
            {Id="2"; Date=date; CurrencyCode="AAA"; FundCompanyId="Company 1"; Quantity=11m; LastChangeDate = changeDate2}
        ]

        let fundRepository = Mock<IFundRepository>()
                                .SetupFunc(fun r -> r.GetFundsToDate(date)).Returns(funds)
                                .Create()
        let logic = BalanceLogic(fundRepository, chronos, idGenerator) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date
        balance.LastUpdateDate |> should equal changeDate1
        balance.FundsByCurrency |> should haveLength 1
        balance.FundsByCurrency[0].CompaniesIds |> should haveLength 2
        balance.FundsByCurrency[0].Quantity |> should equal 21    
