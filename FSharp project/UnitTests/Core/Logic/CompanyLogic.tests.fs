namespace UnitTests.Core.Logic

open System
open NUnit.Framework
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


    [<Test>]
    member this.``Create <when> name is duplicated <should> raises specific error``() =

        let repository = Mock<ICompanyRepository>().Create()
        let logic = CompanyLogic(repository) :> ICompanyLogic

        let company:Company = {Id=""; Name="name"; Types=[CompanyType.Bank]}

        // execute
        let result:Result<Company> = logic.Create(company)
        //result |> should equal (Result<Company>.Ok company)
        result |> should matchResult Result_NotValid

        match result with
        | NotValid message ->
            message |> should contain "name"
            message |> should contain "duplicated"
        | _ -> failwith "expected non valid result"