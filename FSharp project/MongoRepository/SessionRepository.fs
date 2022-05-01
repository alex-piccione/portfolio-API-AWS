﻿namespace Portfolio.MongoRepository

open System
open System.Linq.Expressions
open Portfolio.Core
open Portfolio.Core.Entities
open MongoDB.Driver
open Extensions

type SessionRepository (connectionString:string, database:string, collectionName:string) =
    inherit RepositoryBase<Session>(connectionString, database, collectionName, (fun x -> x.Token))
        
    let createFilter (field:Expression<Func<Session, string>>, value:string):FilterDefinition<Session> =
        FilterDefinitionBuilder<Session>().Eq(field, value)

    new (connectionString:string) = SessionRepository(connectionString, RepositoryBase<_>.DefaultDatabase, "UserSession")

    member this.CreateFilter<'T> (field:Expression<Func<'T, string>>, value:string):FilterDefinition<'T> =
        FilterDefinitionBuilder<'T>().Eq(field, value)

    interface ISessionRepository with
        member this.Find(token: string): Session option = this.Collection.FindOneOrNone(base.IdFilter token)
        member this.Create(session: Session): unit = this.Collection.InsertOne session

        member this.FindByEmail(email: string): Session option =
            this.Collection.FindOneOrNone((fun x -> x.Email = email))

        member this.DeleteExpiredSessions (thresholdDate:DateTime): unit = 
            let filter = FilterDefinitionBuilder<Session>().Lt((fun x -> x.ExpireOn), thresholdDate)
            this.Collection.DeleteMany(filter) |> ignore