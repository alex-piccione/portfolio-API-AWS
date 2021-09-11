namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open MongoDB.Driver

type UserRepository (connectionString:string) as me =
    inherit CrudRepository<User>(connectionString, "User", (fun x -> x.Email))

    let crud = me :> CrudRepository<User>

    interface IUserRepository with
        member this.Create(item: User) = crud.Create item
        member this.Delete(id: string) = crud.Delete id
        member this.Single(id: string) = crud.Single id
        member this.Update(item: User) (id: string) = crud.Update item id
        member this.All() = this.Collection.FindSync(FilterDefinitionBuilder<User>().Empty).ToEnumerable()
