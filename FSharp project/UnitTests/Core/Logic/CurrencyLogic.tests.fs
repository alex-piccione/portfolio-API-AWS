namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open Foq
open Foq.Linq
open FsUnit
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities

type ``CurrencyLogic Test`` () =

    let aCurrency:Currency = { Code="AAA"; Name="Aaa"}
    let fundRepository = Mock.Of<IFundRepository>()

    [<Test>]
    member this.``Create``() =
        let repository = Mock.Of<ICurrencyRepository>()
        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Create aCurrency with
        | Ok c -> 
            c.Code |> should equal aCurrency.Code
            c.Name |> should equal aCurrency.Name
        | _ -> failwith "expected OK result"

    [<Test>]
    member this.``Create [when] Code is Empty [should] raise specific error``() =
        let repository = Mock.Of<ICurrencyRepository>()
        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Create { aCurrency with Code = " "} with
        | Error message -> message |> should contain $"Code cannot be empty."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Create [when] Name is Empty [should] raise specific error``() =
        let repository = Mock.Of<ICurrencyRepository>()
        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Create { aCurrency with Name = " "} with
        | Error message -> message |> should contain $"Name cannot be empty."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Create [when] Code already exists [should] raise specific error``() =
        let code = aCurrency.Code
        let repository = Mock<ICurrencyRepository>()
                             .SetupFunc(fun rep -> rep.ExistsWithCode code).Returns(true)
                             .Create()
        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Create aCurrency with
        | Error message ->
            message |> should contain $"A currency with code \"{code}\" already exists."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Create [when] Name already exists [should] raise specific error``() =
        let name = aCurrency.Name
        let repository = Mock<ICurrencyRepository>()
                             .SetupFunc(fun rep -> rep.ExistsWithName name).Returns(true)
                             .Create()
        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Create aCurrency with
        | Error message ->
            message |> should contain $"A currency with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Single [when] currency exists`` () =
        let code = aCurrency.Code
        let repository = Mock<ICurrencyRepository>()
                             .SetupFunc(fun rep -> rep.Single code).Returns(Some aCurrency)
                             .Create()

        match (CurrencyLogic(repository, fundRepository) :> ICurrencyLogic).Single code with
        | Some c -> c |> should equal aCurrency
        | _ -> failwith "Currency not retrived"

    [<Test>]
    member this.``Single [when] currency NOT exists`` () =
        let code = aCurrency.Code
        let repository = Mock<ICurrencyRepository>()
                                .SetupFunc(fun rep -> rep.Single code).Returns(None)
                                .Create()

        match (CurrencyLogic(repository, fundRepository) :> ICurrencyLogic).Single code with
        | None -> ()
        | Some _ -> failwith "Currency should not be returned"

    [<Test>]
    member this.List () =
        let items = [aCurrency]
        let repository = Mock<ICurrencyRepository>()
                                .SetupFunc(fun rep -> rep.All ()).Returns(items)
                                .Create()

        (CurrencyLogic(repository, fundRepository) :> ICurrencyLogic).List ()
        |> should equal items

        verify <@ repository.All () @> once
(*
     [<Test>]
    member this.``Update``() =
        let name = "test"
        let currency = {aCurrency with Name=name}

        let repository = Mock<ICurrencyRepository>()
                            .SetupFunc(fun rep -> rep.ExistsWithName name).Returns(false)
                            .Create()

        let logic = CurrencyLogic(repository, fundRepository) :> ICurrencyLogic

        // execute
        match logic.Update currency with
        | Ok c -> c.Id |> should equal currency.Id
        | _ -> failwith "expected OK"

        verify <@ repository.Update currency @> once
        

    [<Test>]
    member this.``Update [when] name is Empty [should] raise specific error``() =
        let name = " "
        let company:Company = {Id=Guid.NewGuid().ToString(); Name=name; Types=[Bank]}
        let repository = Mock.Of<ICompanyRepository>()
        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Error message ->
            message |> should contain $"Name cannot be empty."
        | _ -> failwith "expected Error"

    [<Test>]
    member this.``Update [when] name Already Exists [should] raise specific error``() =
        let name = "test"
        let duplicatedName = name.ToUpper()
        let company:Company = {Id=Guid.NewGuid().ToString(); Name=name; Types=[Bank]}
        let existingCompany = {company with Id=Guid.NewGuid().ToString(); Name=duplicatedName}
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.GetByName name).Returns(Some existingCompany)
                             .Create()
        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Error message ->
            message |> should contain $"A company with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"

    




    [<Test>]
    member this.Delete () =
        let repository = Mock<ICompanyRepository>().Create()
        let fundRepository = Mock<IFundRepository>()
                                  .Setup(fun rep -> rep.GetFundsOfCompany (any())).Returns(List.Empty)
                                  .Create()
        match (CompanyLogic(repository, fundRepository) :> ICompanyLogic).Delete "a" with 
        | Ok _ -> ()
        | _ -> failwith "Expect result to be Ok"
        verify <@ repository.Delete "a" @> once

    [<Test>]
    member this.``Delete [when] used in Funds [should] fail`` () =
        let companyId = "Company A"
        let repository = Mock<ICompanyRepository>().Create()
        let funds = [ {FundAtDate.Id="1"; Date=DateTime.Today; CurrencyCode="AAA"; 
            FundCompanyId=companyId; Quantity=1m; LastChangeDate=DateTime.UtcNow } ]
        let fundRepository = Mock<IFundRepository>()
                                  .Setup(fun rep -> rep.GetFundsOfCompany companyId).Returns(funds)
                                  .Create()
        match (CompanyLogic(repository, fundRepository) :> ICompanyLogic).Delete companyId with 
        | Error msg -> msg |> should contain companyId
        | _ -> failwith "Expect result to be Error"

        verify <@ repository.Delete companyId @> never
        *)