namespace Portfolio.Api.MongoRepository

open System
open System.Linq.Expressions
open MongoDB.Driver
open MongoDB.Bson.Serialization

type RepositoryBase<'T>(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>) =

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
            BsonClassMap.RegisterClassMap(map);

    member this.IdFilter id = FilterDefinitionBuilder<'T>().Eq(idField, id);
    member this.Collection = database.GetCollection<'T>(collectionName);


type CrudRepository<'T>(connectionString:string, collectionName:string, idField:Expression<Func<'T, string>>) =
    inherit RepositoryBase<'T>(connectionString, collectionName, idField)

    member this.Create(item: 'T): unit = this.Collection.InsertOne item
    member this.Delete(id: string): unit = this.Collection.DeleteOne(this.IdFilter id) |> ignore
    member this.Single(id: string): 'T option= 
        let f = this.Collection.Find(this.IdFilter id)
        match f.CountDocuments() with
        | 1L -> Some(f.Single())
        | 0L -> None
        | _ -> None
        //match this.Collection.Find(this.IdFilter id).SingleOrDefault() with
        //| d when (idField d) = ""
        //| _ -> None
    member this.Update(id: string, item: 'T): unit = this.Collection.ReplaceOne(this.IdFilter id, item) |> ignore

    member this.All() = this.Collection.FindSync(FilterDefinitionBuilder<'T>().Empty).ToEnumerable()

