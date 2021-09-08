namespace IntegrationTests

open NUnit.Framework

type CurrencyRepository () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    member this.Create () =



        Assert.Pass()
