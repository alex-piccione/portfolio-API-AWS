namespace Portfolio.MongoRepository

open System
open System.Linq.Expressions
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver
open Extensions

type SessionRepository (connectionString:string) =
    inherit RepositoryBase<Session>(connectionString, "UserSession", (fun x -> x.Token))
    
    let createFilter (field:Expression<Func<Session, string>>, value:string):FilterDefinition<Session> =
        FilterDefinitionBuilder<Session>().Eq(field, value)

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    interface ISessionRepository with
        member this.Find(token: string): Session option = this.Collection.FindOneOrNone(base.IdFilter token)
        member this.Create(session: Session): unit = this.Collection.InsertOne session

        member this.FindByEmail(email: string): Session option =
            //let expr:Expression<Func<Session, string>> = fun x -> x.
            //let a:Expression<Func<'T, string>> = (fun x -> x.Email)
            //let f = createFilter( (fun x -> x.Email) , email)
            let filter = this.CreateFilter( (fun x -> x.Email) , email)
            this.Collection.FindOneOrNone(filter)

        member this.DeleteExpiredSessions (thresholdDate:DateTime): unit = 
            let filter = FilterDefinitionBuilder<Session>().Lt((fun x -> x.ExpireOn), thresholdDate)
            this.Collection.DeleteMany(filter) |> ignore