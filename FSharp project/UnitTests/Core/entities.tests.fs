namespace Portfolio.UnitTests

open System.Text.Json
open NUnit.Framework
open FsUnit
open Portfolio.Core.Entities

type ``Company and CompanyType`` () =

    [<Test>]
    member this.``CompanyType Parse <should> resiolve its members`` () =
        CompanyType.Parse(CompanyType.Bank.ToString()) |> should equal CompanyType.Bank
        CompanyType.Parse(CompanyType.Exchange.ToString()) |> should equal CompanyType.Exchange
        CompanyType.Parse(CompanyType.Stacking.ToString()) |> should equal CompanyType.Stacking


