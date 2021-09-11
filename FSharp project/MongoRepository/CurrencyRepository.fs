namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open MongoDB.Driver

type CurrencyRepository (connectionString:string) as this =
    inherit CrudRepository<Currency>(connectionString, "Currency", (fun x -> x.Code))

    let crud = this :> CRUD<Currency>

    interface ICurrencyRepository with 
        member this.Create(item: Currency) = (this :> CRUD<Currency>).Create item
        member this.Delete(id: string) = crud.Delete id
        member this.Single(id: string) = crud.Single id
        member this.Update(item: Currency) (id: string) = crud.Update item id
        member this.All() = this.Collection.FindSync(FilterDefinitionBuilder<Currency>().Empty).ToEnumerable()
