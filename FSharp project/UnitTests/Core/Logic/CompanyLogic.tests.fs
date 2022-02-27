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

    [<Test>]
    member this.``Create``() =
        let repository = Mock.Of<ICompanyRepository>()
        let logic = CompanyLogic(repository) :> ICompanyLogic
        let company:Company = {Id=""; Name="a name"; Types=[CompanyType.Bank]}

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
        let logic = CompanyLogic(repository) :> ICompanyLogic

        let company:Company = {Id=""; Name=name; Types=[CompanyType.Bank]}

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
        let logic = CompanyLogic(repository) :> ICompanyLogic

        let company:Company = {Id=""; Name=name; Types=[CompanyType.Bank]}

        // execute
        match logic.Create company with
        | Error message ->
            message |> should contain $"A company with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"

    [<Test>]
    member this.``Update``() =
        let name = "test"
        let company:Company = {Id=newGuid(); Name=name; Types=[CompanyType.Bank]}

        let repository = Mock<ICompanyRepository>()
                                 .SetupFunc(fun rep -> rep.GetByName name).Returns(None)
                                 //.SetupFunc(fun rep -> rep.Update company).Returns(ignore)
                                 .Create()

        let logic = CompanyLogic(repository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Ok c -> c.Id |> should equal company.Id
        | _ -> failwith "expected OK"

        verify <@ repository.Update company @> once

    [<Test>]
    member this.``Update <when> name is Empty <should> raise specific error``() =
        let name = " "
        let company:Company = {Id=Guid.NewGuid().ToString(); Name=name; Types=[CompanyType.Bank]}
        let repository = Mock.Of<ICompanyRepository>()
        let logic = CompanyLogic(repository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Error message ->
            message |> should contain $"Name cannot be empty."
        | _ -> failwith "expected Error"

    [<Test>]
    member this.``Update <when> name Already Exists <should> raise specific error``() =
        let name = "test"
        let duplicatedName = name.ToUpper()
        let company:Company = {Id=Guid.NewGuid().ToString(); Name=name; Types=[CompanyType.Bank]}
        let existingCompany = {company with Id=Guid.NewGuid().ToString(); Name=duplicatedName}
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.GetByName name).Returns(Some existingCompany)
                             .Create()
        let logic = CompanyLogic(repository) :> ICompanyLogic

        // execute
        match logic.Update company with
        | Error message ->
            message |> should contain $"A company with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"


    [<Test>]
    member this.Single () =
        failwith "not implemented"

    [<Test>]
    member this.List () =
        failwith "not implemented"

    [<Test>]
    member this.Delete () =
        failwith "not implemented"