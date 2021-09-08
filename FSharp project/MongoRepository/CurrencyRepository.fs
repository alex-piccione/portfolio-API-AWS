namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type CurrencyRepository (conenctionString:string) =

    interface ICurrencyRepository with

        member this.Create(arg1: Currency): unit = 
            ()

        member this.Get(code: string): Currency = 
            { Code=code; Name=code.ToLowerInvariant() }

        member this.List(): Currency list = 
            [
                { Code="EUR"; Name="Euro"}
                { Code="XRP"; Name="Ripple"}
            ]