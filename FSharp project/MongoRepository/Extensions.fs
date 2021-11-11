module Extensions

open System
open System.Linq.Expressions
open MongoDB.Driver

type IMongoCollection<'T> with

    member this.FindOneOrNone(filter:FilterDefinition<'T>) =
        let f = this.Find(filter)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | _ -> None

    member this.FindOneOrNone(filter:Expression<Func<'T, bool>>) =
        let f = this.Find(filter)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | _ -> None