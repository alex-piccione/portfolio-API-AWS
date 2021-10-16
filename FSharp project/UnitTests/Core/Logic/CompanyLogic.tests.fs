namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open Foq
open Foq.Linq
open FsUnit
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities
open NUnit.Framework.Constraints


type matchingResult = Result_Ok | Result_Error | Result_NotValid


type matchResult(expected: matchingResult) =
    inherit Constraints.EqualConstraint(expected)

    override this.ApplyTo actual =
        let pass = 
            match box actual with
            | :? Result<Company> as result -> 
                match result with
                | Ok c -> expected = Result_Ok
                | Error s -> expected = Result_Error
                | NotValid s -> expected = Result_NotValid
            | _ -> failwith "passed type must be Result<Company>"
        
        ConstraintResult(this, actual, pass)

type ``CompanyLogic Test`` () =

    let newGuid () = Guid.NewGuid().ToString();

    [<Test>]
    member this.``Create <when> name is duplicated <should> raise specific error``() =

        let name = "test"
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.Exists name).Returns(true)
                             .Create()
        let logic = CompanyLogic(repository) :> ICompanyLogic

        let company:Company = {Id=""; Name=name; Types=[CompanyType.Bank]}

        // execute
        let result:Result<Company> = logic.Create(company)

        result |> should matchResult Result_NotValid

        match result with
        | NotValid message ->
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
        let result:Result<Company> = logic.Update(company)

        match result with
        | Ok c -> c.Id |> should equal company.Id
        | _ -> failwith "expected OK"

        verify <@ repository.Update company @> once


    [<Test>]
    member this.``Update <when> name is duplicated <should> raise specific error``() =

        let name = "test"
        let duplicatedName = name.ToUpper()
        let company:Company = {Id=Guid.NewGuid().ToString(); Name=name; Types=[CompanyType.Bank]}
        let existingCompany = {company with Id=Guid.NewGuid().ToString(); Name=duplicatedName}
        let repository = Mock<ICompanyRepository>()
                             .SetupFunc(fun rep -> rep.GetByName name).Returns(Some existingCompany)
                             .Create()
        let logic = CompanyLogic(repository) :> ICompanyLogic

        // execute
        let result:Result<Company> = logic.Update(company)

        result |> should matchResult Result_NotValid

        match result with
        | NotValid message ->
            message |> should contain $"A company with name \"{name}\" already exists."
        | _ -> failwith "expected non valid result"