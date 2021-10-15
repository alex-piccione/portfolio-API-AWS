namespace UnitTests.Core.Logic

open System
open NUnit.Framework
open FsUnit
open Foq.Linq
open Portfolio.Core
open Portfolio.Core.Logic
open Portfolio.Core.Entities


type BalanceLogicTest() =

    //let mutable logic = BalanceLogic()

    [<SetUp>]
    member this.SetUp() =
        ()

    // TODO: implement in the right branch
    //[<Test>]
    member this.``GetBalance``() =

        let date = DateTime(2010, 08, 15)

        let currencies:Currency list = [{Code="AAA"; Name="Aaa"}; {Code="BBB"; Name="Bbb"}]
        let currencyRepository = Mock<ICurrencyRepository>().Setup(fun rep -> rep.All()).Returns(currencies).Create()
        let fundRepository = Mock<IFundRepository>().Create()
        let logic = BalanceLogic(fundRepository, currencyRepository) :> IBalanceLogic

        // execute
        let balance = logic.GetBalance date

        balance.Date |> should equal date

        ()

