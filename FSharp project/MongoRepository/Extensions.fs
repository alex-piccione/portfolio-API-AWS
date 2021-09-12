module Extensions

open MongoDB.Driver
   
type IMongoCollection<'T> with
    member this.FindOneOrNone(filter:FilterDefinition<'T>) =
        let f = this.Find(filter)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | 0L -> None
        | _ -> None