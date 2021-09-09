namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type CurrencyRepository (conenctionString:string) =

    interface ICurrencyRepository with
        member this.Delete(arg1: string): unit = 
            raise (System.NotImplementedException())
        member this.Update(arg1: Currency, arg2: string): unit = 
            raise (System.NotImplementedException())

        member this.Create(currency: Currency): unit = 
            ()

        member this.Single(code: string): Currency = 
            { Code=code; Name=code.ToLowerInvariant() }

        member this.All(): Currency list = 
            [
                { Code="EUR"; Name="Euro"}
                { Code="XRP"; Name="Ripple"}
            ]