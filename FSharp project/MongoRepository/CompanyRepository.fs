namespace Portfolio.MongoRepository

open Portfolio.Core
open Portfolio.Core.Entities

type CompanyRepository (connectionString:string) =
    inherit CrudRepository<Company>(connectionString, "Company", (fun x -> x.Id))

    interface ICompanyRepository with 
        member this.Create(item: Company) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: Company) = base.Update(item.Code, item)
