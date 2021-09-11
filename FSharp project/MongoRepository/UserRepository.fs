namespace Portfolio.Api.MongoRepository

open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type UserRepository (connectionString:string) =
    inherit CrudRepository<User>(connectionString, "User", (fun x -> x.Email))

    interface IUserRepository with
        member this.Create(item: User) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id        
        member this.Update(item: User) = base.Update(item.Email, item)
        member this.All() = base.All()