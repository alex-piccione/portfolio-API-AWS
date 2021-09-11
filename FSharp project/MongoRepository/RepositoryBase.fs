namespace Portfolio.Api.MongoRepository

open System
open MongoDB.Driver
open MongoDB.Bson.Serialization

type RepositoryBase<'T>(connectionString:string, collectionName:string,
                        idField:System.Linq.Expressions.Expression<System.Func<'T, string>> 
                        ) =

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

type CrudRepository<'T>(connectionString:string, collectionName:string,
                        idField:System.Linq.Expressions.Expression<System.Func<'T, string>>) =
    inherit RepositoryBase<'T>(connectionString, collectionName, idField)

    member this.Create(item: 'T): unit = this.Collection.InsertOne item
    member this.Delete(id: string): unit = this.Collection.DeleteOne(this.IdFilter id) |> ignore
    member this.Single(id: string): 'T = this.Collection.FindSync(this.IdFilter id).FirstOrDefault()
    member this.Update (item: 'T) (id: string) = this.Collection.FindOneAndReplace(this.IdFilter id, item)
