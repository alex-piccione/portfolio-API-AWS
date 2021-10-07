namespace Portfolio.MongoRepository

open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Bson.Serialization
open serializers


type CompanyRepository (connectionString:string) =
    inherit CrudRepository<Company>(connectionString, "Company", (fun x -> x.Id), CompanyRepository.overloadMap)

    interface ICompanyRepository with 
        member this.Create(item: Company) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: Company) = base.Update(item.Id, item)

    static member overloadMap (map:BsonClassMap<Company>) = 
        map.MapProperty("Types").SetSerializer(CompanyTypeSerializer()) |> ignore
