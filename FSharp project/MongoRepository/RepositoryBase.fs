namespace Portfolio.MongoRepository

open System
open System.Linq.Expressions
open MongoDB.Driver
open MongoDB.Bson.Serialization

type RepositoryBase<'T>(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>, overloadMap: BsonClassMap<'T> -> unit) =

    let DATABASE = "Portfolio"

    let client = new MongoClient(MongoClientSettings.FromConnectionString(connectionString))
    // setting the Timeout returns this error: MongoClientSettings is frozen
    //client.Settings.ConnectTimeout = TimeSpan.FromSeconds(30); 
    let databaseSettings = new MongoDatabaseSettings() // default settings
    let database = client.GetDatabase(DATABASE, databaseSettings)

    do 
        if not(BsonClassMap.IsClassMapRegistered(typeof<'T>)) then
            let map = BsonClassMap<'T>()
            map.AutoMap()
            map.MapIdMember(idField) |> ignore
            map.SetIgnoreExtraElements true
            overloadMap map
            BsonClassMap.RegisterClassMap(map)

    new(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>) =
        RepositoryBase(connectionString, collectionName, idField, fun x -> ())

    member this.IdFilter id = FilterDefinitionBuilder<'T>().Eq(idField, id);
    member this.Collection = database.GetCollection<'T>(collectionName);
    member this.Filter () = FilterDefinitionBuilder<'T>()
    // member this.FilterEq () = FilterDefinitionBuilder<'T>().Eq()


type CrudRepository<'T>(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>, overloadMap: BsonClassMap<'T> -> unit) =
    inherit RepositoryBase<'T>(connectionString, collectionName, idField, overloadMap)

    new(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>) =
        CrudRepository<'T>(connectionString, collectionName, idField, fun map -> ())

    member this.Create(item: 'T): unit = this.Collection.InsertOne item
    member this.Delete(id: string): unit = this.Collection.DeleteOne(this.IdFilter id) |> ignore
    member this.Single(id: string): 'T option= 
        let f = this.Collection.Find(this.IdFilter id)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | 0L -> None
        | _ -> failwith "More than one record found"
        //match this.Collection.Find(this.IdFilter id).SingleOrDefault() with
        //| d when (idField d) = ""
        //| _ -> None
    member this.Update(id: string, item: 'T): unit = this.Collection.ReplaceOne(this.IdFilter id, item) |> ignore

    member this.All() = this.Collection.FindSync(FilterDefinitionBuilder<'T>().Empty).ToEnumerable()

