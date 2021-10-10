namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open Foq.Linq
open Portfolio.Core.Logic
open Portfolio.Core



type ``CompanyLogic Test`` () =

    [<Test>]
    member this.``Create <when> name is duplicated <should> raises specific error``() =

        let repository = Mock<ICompanyRepository>
        let logic = CompanyLogic(repository)

        // execute
        //logic.Create(company)

        ()

