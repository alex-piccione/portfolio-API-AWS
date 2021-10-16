namespace Portfolio.MongoRepository

open System.Linq
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver
open MongoDB.Bson.Serialization
open serializers


type CompanyRepository (connectionString:string) =
    inherit CrudRepository<Company>(connectionString, "Company", (fun x -> x.Id), CompanyRepository.overloadMap)

    interface ICompanyRepository with
        member this.Create(item: Company) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: Company) = base.Update(item.Id, item)
        member this.All() = base.All()
        member this.Exists(name: string): bool = 
            let nameToSearch = name.ToLowerInvariant()
            let filter = Builders<Company>.Filter.Where(fun x -> x.Name.ToLowerInvariant() = nameToSearch)
            base.Collection.FindAsync(filter).Result.ToEnumerable().Any()
        member this.GetByName(name: string) = 
            let nameToSearch = name.ToLowerInvariant()
            let filter = Builders<Company>.Filter.Where(fun x -> x.Name.ToLowerInvariant() = nameToSearch)
            
            let records = base.Collection.Find filter
            match records.CountDocuments() with
            | 1L -> Some(records.Single())
            | 0L -> None
            | _ -> failwith "More than one record found"

    static member overloadMap (map:BsonClassMap<Company>) = 
        map.MapProperty("Types").SetSerializer(CompanyTypeSerializer()) |> ignore
