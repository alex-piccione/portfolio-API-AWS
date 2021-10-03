namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type CompanyRepository (connectionString:string) =
    inherit CrudRepository<Company>(connectionString, "Company", (fun x -> x.Code))

    interface ICompanyRepository with 
        member this.Create(item: Currency) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: Currency) = base.Update(item.Code, item)
