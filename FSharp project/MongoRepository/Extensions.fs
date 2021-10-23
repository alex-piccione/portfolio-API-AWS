module Extensions

open System.Linq
open System.Collections.Generic
open MongoDB.Driver
   
type IMongoCollection<'T> with
    member this.FindOneOrNone(filter:FilterDefinition<'T>) =
        let f = this.Find(filter)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | 0L -> None
        | _ -> None

(*
// when 'a : null
type IEnumerable<'a> with
    member this.FirstOrNone ()  =
        match this.FirstOrDefault() with
        | null-> None
        //| x when isNull(x) -> None
        | item -> Some item
*)