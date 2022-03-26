namespace Portfolio.MongoRepository

open System.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver

type CurrencyRepository (connectionString:string, collectionName:string) =
    inherit CrudRepository<Currency>(connectionString, collectionName, (fun x -> x.Code))

    new (connectionString:string) = CurrencyRepository(connectionString, "Currency")

    interface ICurrencyRepository with 
        member this.Create(item: Currency) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: Currency) = base.Update(item.Code, item)
        member this.All() = base.All()

        member this.ExistsWithCode(code: string): bool = 
            let codeToSearch = code.ToLowerInvariant()
            let filter = Builders<Currency>.Filter.Where(fun x -> x.Code.ToLowerInvariant() = codeToSearch)
            base.Collection.FindAsync(filter).Result.ToEnumerable().Any()

        member this.ExistsWithName(name: string): bool = 
            let nameToSearch = name.ToLowerInvariant()
            let filter = Builders<Currency>.Filter.Where(fun x -> x.Name.ToLowerInvariant() = nameToSearch)
            base.Collection.FindAsync(filter).Result.ToEnumerable().Any()