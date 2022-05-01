namespace Portfolio.MongoRepository

open Portfolio.Core
open Portfolio.Core.Entities

type UserRepository (connectionString:string, database:string, collectionName:string) =
    inherit CrudRepository<User>(connectionString, database, collectionName, (fun x -> x.Email))

    new (connectionString:string) = UserRepository(connectionString, RepositoryBase<_>.DefaultDatabase, "User")

    interface IUserRepository with
        member this.Create(item: User) = base.Create item
        member this.Delete(id: string) = base.Delete id
        member this.Single(id: string) = base.Single id
        member this.Update(item: User) = base.Update(item.Email, item)
        member this.All() = base.All()