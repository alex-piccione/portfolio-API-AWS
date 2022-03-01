namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open Foq
open Foq.Linq
open FsUnit
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities
open match_helper
open NUnit.Framework.Constraints

type ``CompanyLogic Test`` () =

    let newGuid () = Guid.NewGuid().ToString();
    let shouldReturnOk = fun x -> x should matchResult<Company> Result_Ok 
    let shouldReturnError = fun x -> x should matchResult<Company> Result_Error

    //let returnError x = x -> matchResult<Company> Result_Error // ('a -> EqualConstraint) -> obj = matchResult<Company> Result_Error

    // should is 
    // f: ('a -> #Constraint) -> x:'a -> y:obj -> unit
    //EqualConstraint

    let aCompany:Company = { Id="TEST"; Name="Company"; Types=[Bank; Exchange]}


    let fundRepository = Mock.Of<IFundRepository>()


    [<Test>]
    member this.``Create``() =
        let repository = Mock.Of<ICompanyRepository>()
        let fundRepository = Mock.Of<IFundRepository>()
        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic
        let company:Company = {Id=""; Name="a name"; Types=[Bank]}

        // execute
        match logic.Create company with
        | Ok comp -> 
            comp.Id |> should not' (be NullOrEmptyString)
            comp.Name |> should equal company.Name
            comp.Types |> should equivalent company.Types
        | _ -> failwith "expected OK result"

    [<Test>]
    member this.``Create <when> name is Empty <should> raise specific error``() =
        let name = " "
        let repository = Mock.Of<ICompanyRepository>()
        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic

        let company:Company = {Id=""; Name=name; Types=[Bank]}

        // execute
        match logic.Create company with
        | Error message -> message |> should contain $"Name cannot be empty."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Create <when> name Already Exists <should> raise specific error``() =

        let name = "test"
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.Exists name).Returns(true)
                             .Create()
        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic

        let company:Company = {Id=""; Name=name; Types=[Bank]}

        // execute
        match logic.Create company with
        | Error message ->
            message |> should contain $"A company with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Update``() =
        let name = "test"
        let company:Company = {Id=newGuid(); Name=name; Types=[Bank]}

        let repository = Mock<ICompanyRepository>()
                                 .SetupFunc(fun rep -> rep.GetByName name).Returns(None)
                                 //.SetupFunc(fun rep -> rep.Update company).Returns(ignore)
                                 .Create()

        let logic = CompanyLogic(repository, fundRepository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Ok c -> c.Id |> should equal company.Id
        | _ -> failwith "expected OK"

        verify <@ repository.Update company @> once

    [<Test>]
    member this.``Update <when> name is Empty <should> raise specific error``() =
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
    member this.``Update <when> name Already Exists <should> raise specific error``() =
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
    member this.``Single [when] company exists`` () =
        let company = aCompany

        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.Single company.Id).Returns(Some company)
                             .Create()

        match (CompanyLogic(repository, fundRepository) :> ICompanyLogic).Single company.Id with
        | Some c -> c |> should equal aCompany
        | _ -> failwith "Company not etrived"

    [<Test>]
    member this.``Single [when] company NOT exists`` () =
        let company = aCompany
        let repository = Mock<ICompanyRepository>()
                                .SetupFunc(fun rep -> rep.Single (company.Id)).Returns(None)
                                .Create()

        match (CompanyLogic(repository, fundRepository) :> ICompanyLogic).Single company.Id with
        | None -> ()
        | Some _ -> failwith "Company should not be returned"

    [<Test>]
    member this.List () =
        let companies = [aCompany]
        let repository = Mock<ICompanyRepository>()
                                .SetupFunc(fun rep -> rep.All ()).Returns(companies)
                                .Create()

        (CompanyLogic(repository, fundRepository) :> ICompanyLogic).List ()
        |> should equal companies

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
    member this.``Delete [when] used in Funds [shoould] fail`` () =
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
