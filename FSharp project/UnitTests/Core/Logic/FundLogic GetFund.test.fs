namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open FsUnit
open Foq
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities
open NUnit.Framework.Constraints
           
type ``BalanceLogic GetFund Test``() =
    let Now = DateTime.UtcNow
    let chronos = Mock<IChronos>().SetupPropertyGet(fun c -> c.Now).Returns(Now).Create()
    let idGenerator = Mock<IIdGenerator>().Create()

    [<Test>]
    member this.``GetFundOfCurrencyByDate [with] simple case``() =
        let currencyCode = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let date1 = new DateTime(2000, 01, 01)
        let date2 = new DateTime(2000, 02, 01)
        let updateDate = new DateTime(2000, 04, 05)
        let company1 = "C1"
        let company2 = "C2"

        let fundAtDate:FundAtDate = { Id=""; Date=date1; CurrencyCode=currencyCode; FundCompanyId="c1"; Quantity=1m; LastChangeDate=updateDate }

        //   company1: d1 d2  -> d1,  d2
        //   company2: d1 --  -> d1, (d2)
        let funds = [
            {fundAtDate with Id="1"; Date=date1; FundCompanyId=company1; Quantity=1m}
            {fundAtDate with Id="2"; Date=date1; FundCompanyId=company2; Quantity=2m}
            {fundAtDate with Id="3"; Date=date2; FundCompanyId=company1; Quantity=4m}
        ]

        let record:CompanyFund = {Id=None; CompanyId=""; Quantity=0m; LastUpdateDate=updateDate}
        let expectedResult:CurrencyFundAtDate list = [
                {Date=date1; TotalQuantity=3m; CompanyFunds=[
                    {record with Id=Some("1"); CompanyId=company1; Quantity=1m;}
                    {record with Id=Some("2"); CompanyId=company2; Quantity=2m;}  
                ]}
                {Date=date2; TotalQuantity=6m; CompanyFunds=[                    
                    {record with Id=Some("3"); CompanyId=company1; Quantity=4m;}                
                    {record with Id=None; CompanyId=company2; Quantity=2m;} 
                ]}
            ]
        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.GetFundsOfCurrency(currencyCode, minDate))
                                .Returns(funds)
                                .Create()
        // execute
        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

        logic.GetFundOfCurrencyByDate(currencyCode, minDate) |> should equalCurrencyFundAtDateList expectedResult
        verify <@ fundRepository.GetFundsOfCurrency(currencyCode, minDate) @> once

    [<Test>]
    member this.``GetFundOfCurrencyByDate [with] complex case``() =
        let currencyCode = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let date1 = new DateTime(2000, 01, 01)
        let date2 = new DateTime(2000, 02, 01)
        let date3 = new DateTime(2000, 03, 01)
        let date4 = new DateTime(2000, 04, 01)
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
            {fundAtDate with Id="6"; Date=date4; FundCompanyId=company2; Quantity=40m} 
        ]

        let record:CompanyFund = {Id=None; CompanyId=""; Quantity=0m; LastUpdateDate=updateDate}
        let expectedResult:CurrencyFundAtDate list = [
                {Date=date1; TotalQuantity=1m; CompanyFunds=[
                    {record with Id=Some("1"); CompanyId=company1; Quantity=1m;}                    
                ]}
                {Date=date2; TotalQuantity=22m; CompanyFunds=[
                    {record with Id=Some("3"); CompanyId=company1; Quantity=2m;}
                    {record with Id=Some("4"); CompanyId=company2; Quantity=20m;}                    
                ]}
                {Date=date3; TotalQuantity=32m; CompanyFunds=[
                    {record with Id=None; CompanyId=company1; Quantity=2m;}
                    {record with Id=Some("5"); CompanyId=company2; Quantity=30m;}                    
                ]}
                {Date=date4; TotalQuantity=42m; CompanyFunds=[
                    {record with Id=None; CompanyId=company1; Quantity=2m;}
                    {record with Id=Some("6"); CompanyId=company2; Quantity=40m;}                    
                ]}
            ]
        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.GetFundsOfCurrency(currencyCode, minDate))
                                .Returns(funds)
                                .Create()
        // execute
        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

        logic.GetFundOfCurrencyByDate(currencyCode, minDate) |> should equalCurrencyFundAtDateList expectedResult

        verify <@ fundRepository.GetFundsOfCurrency(currencyCode, minDate) @> once

    [<Test>]
    member this.``GetFundOfCurrencyByDate [when] Quantity is set to 0``() =
        let currencyCode = "AAA"
        let minDate = DateTime(2000, 1, 1)

        let date1 = new DateTime(2000, 01, 01)
        let date2 = new DateTime(2000, 02, 01)
        let date3 = new DateTime(2000, 03, 01)
        let updateDate = new DateTime(2000, 04, 05)
        let company1 = "C1"
        let company2 = "C2"

        let fundAtDate:FundAtDate = { Id=""; Date=date1; CurrencyCode=currencyCode; FundCompanyId="c1"; Quantity=1m; LastChangeDate=updateDate }

        let funds = [
            {fundAtDate with Id="1"; Date=date1; FundCompanyId=company1; Quantity=1m}
            {fundAtDate with Id="2"; Date=date1; FundCompanyId=company2; Quantity=2m}
            {fundAtDate with Id="3"; Date=date2; FundCompanyId=company1; Quantity=0m} // set to 0
            {fundAtDate with Id="4"; Date=date3; FundCompanyId=company2; Quantity=3m}
        ]

        let record:CompanyFund = {Id=None; CompanyId=""; Quantity=0m; LastUpdateDate=updateDate}
        let expectedResult:CurrencyFundAtDate list = [
                {Date=date1; TotalQuantity=3m; CompanyFunds=[
                    {record with Id=Some("1"); CompanyId=company1; Quantity=1m;}
                    {record with Id=Some("2"); CompanyId=company2; Quantity=2m;}  
                ]}
                {Date=date2; TotalQuantity=2m; CompanyFunds=[                    
                    {record with Id=Some("3"); CompanyId=company1; Quantity=0m;}                
                    {record with Id=None; CompanyId=company2; Quantity=2m;} 
                ]}
                {Date=date3; TotalQuantity=3m; CompanyFunds=[                    
                    //{record with Id=None; CompanyId=company1; Quantity=0m;}                
                    {record with Id=Some("4"); CompanyId=company2; Quantity=3m;} 
                ]}
            ]
        let fundRepository = Mock<IFundRepository>()
                                .Setup(fun r -> r.GetFundsOfCurrency(currencyCode, minDate))
                                .Returns(funds)
                                .Create()
        // execute
        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

        logic.GetFundOfCurrencyByDate(currencyCode, minDate) |> should equalCurrencyFundAtDateList expectedResult
        verify <@ fundRepository.GetFundsOfCurrency(currencyCode, minDate) @> once


and equalCurrencyFundAtDateList(expected:CurrencyFundAtDate list) = 
    inherit Constraints.EqualConstraint(expected)
    
    override this.ApplyTo<'C> (actualObject:'C): ConstraintResult =
        let actual = box actualObject :?> CurrencyFundAtDate list

        let checkDate (expDate:CurrencyFundAtDate) =
            // check Date exists
            match actual |> List.tryFind (fun ac -> ac.Date = expDate.Date) with 
            | None -> Some $"Date {expDate.Date} not found"
            | Some currDate -> 
                // check TotalQuantity
                match expDate.TotalQuantity = currDate.TotalQuantity with
                | false -> Some $"Quantity should be {expDate.TotalQuantity} bus is {currDate.TotalQuantity} for Date {expDate.Date}"
                | _ ->
                    // check Companies
                    match expDate.CompanyFunds.Length = currDate.CompanyFunds.Length with 
                    | false -> Some $"Expected {expDate.CompanyFunds.Length} CompanyFunds but foud {currDate.CompanyFunds.Length} for Date {expDate.Date}"
                    | _ -> 
                        expDate.CompanyFunds 
                        |> List.tryPick(
                            fun expectedFund -> 
                                match expDate.CompanyFunds |> List.tryFind (fun c -> c.CompanyId = expectedFund.CompanyId) with
                                | None -> Some $"Company ${expectedFund.CompanyId} not found in Date {expDate.Date}"
                                | Some actualFund -> 
                                    match expectedFund = actualFund with                                
                                    | false -> Some $"Fund for CompanyId {expectedFund.CompanyId} does not match for Date {expDate.Date}"
                                    | _ -> None                                    
                                )                        

        // check number
        if not (actual.Length = expected.Length) then
            ConstraintResult(this, $"Items count should be {expected.Length} but is {actual.Length}", false)
        else
            // check all the dates
            match expected |> List.tryPick checkDate with
            | Some error -> ConstraintResult(this, error, false)
            | None -> ConstraintResult(this, null, true)