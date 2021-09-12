module SessionManager

open System
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities

type ISessionManager =
    abstract member GetSession: email:string -> Session
    abstract member CleanupExpiredSessions: unit -> unit

type SessionManager (sessionRepository:ISessionRepository) =

    let SESSION_HIDLE = TimeSpan.FromMinutes 30.
    let EXPIRED_CLEANUP_THRESHOLD = TimeSpan.FromDays 1.

    let createSession email =
        let token = Guid.NewGuid().ToString()
        let session:Session = { 
            Email=email
            Token=token 
            CreatedOn=DateTime.UtcNow
            ExpireOn=DateTime.UtcNow + SESSION_HIDLE
        }
        sessionRepository.Create session
        session

    interface ISessionManager with
        member this.CleanupExpiredSessions(): unit = sessionRepository.DeleteExpiredSessions (DateTime.UtcNow - EXPIRED_CLEANUP_THRESHOLD)

        member this.GetSession email =
            try 
                match sessionRepository.FindByEmail email with
                | None -> createSession email
                | Some session when session.ExpireOn > DateTime.UtcNow -> createSession email
                | Some session -> session
            with exc -> failwith $"Failed to retrieve or create Session. {exc}"
