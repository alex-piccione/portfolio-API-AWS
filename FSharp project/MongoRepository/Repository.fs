namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type MongoRepository () =

    interface ICurrencyRepository with

        member this.Create(arg1: Currency): unit = 
            ()

        member this.Get(arg1: string): Currency = 
            { Code="EUR"; Name="Euro"}

        member this.List(): Currency list = 
            [
                { Code="EUR"; Name="Euro"}
                { Code="XRP"; Name="Ripple"}
            ]