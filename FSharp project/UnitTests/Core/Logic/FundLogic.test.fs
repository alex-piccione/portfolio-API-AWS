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

    let testRequestValidation request expectedError = 
        let error method = Exception($"call to \"{method}\" not expected")
        let mockRepository = 
            Mock<IFundRepository>()
                .Setup(fun r -> r.FindFundAtDate (any()) ).Raises(error "FindFundAtDate") 
                .Setup(fun r -> r.CreateFundAtDate (any()) ).Raises(error "CreateFundAtDate") 
                .Setup(fun r -> r.UpdateFundAtDate (any()) ).Raises(error "UpdateFundAtDate") 
                .Create()

        match (FundLogic(mockRepository, chronos, idGenerator) :> IFundLogic).CreateOrUpdate(request) with
        | Error error -> error |> should equal expectedError
        | x -> x |> failwith $"unexpected result: {x} instead of {Error}"

    [<Test>]
    member this.``CreateOrUpdate [when] record exists [should] update`` () =
        let fundRepository = Mock<IFundRepository>()
                                 .Setup(fun r -> r.FindFundAtDate (any()))
                                 .Returns(Some(fundAtDate))
                                 .Create()
        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

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

        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

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
        let logic = FundLogic(fundRepository, chronos, idGenerator) :> IFundLogic

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
        let expectedError = validation.mustBeDefined "Date"
        testRequestValidation request expectedError

    [<Test>]
    member this.``CreateOrUpdate [when] Date is in the future [should] return error``() =
        testRequestValidation 
            {request with Date = DateTime.UtcNow.AddDays(1)}
            (validation.mustBeInThePast "Date")

    [<Test>]
    member this.``CreateOrUpdate [when] Currency is missing [should] return error``() =
        testRequestValidation
            {request with CurrencyCode = ""}
            (validation.mustBeDefined "Currency")

    [<Test>]
    member this.``CreateOrUpdate [when] Quantity is not positive [should] return error``() =
        testRequestValidation
            {request with Quantity = 0m}
            (validation.mustBeGreaterThanZero "Quantity")

    [<Test>]
    member this.``CreateOrUpdate [when] Company is missing [should] return error``() =
        testRequestValidation
            {request with CompanyId = ""}
            (validation.mustBeDefined "Company")


and equalResult(expected:Result<_,_>) = 
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo<'C> (actual:'C): ConstraintResult =   
        match box actual with 
        | :? Result<FundUpdateResult,string> as result -> 
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