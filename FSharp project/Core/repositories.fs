namespace Portfolio.Api.Core

open System
open Portfolio.Api.Core.Entities
open System.Collections.Generic

type CRUD<'T> =
    abstract member Create: 'T -> unit
    abstract member Single: string -> 'T option
    abstract member Update: 'T -> unit
    abstract member Delete: string -> unit

type All<'T> =
    abstract member All: unit -> IEnumerable<'T>

type List<'T> =
    abstract member List: skip:int -> take:int -> IEnumerable<'T>

type IUserRepository =
    inherit CRUD<User>
    inherit All<User>

type ICurrencyRepository =
    inherit CRUD<Currency>
    inherit All<Currency>

type ISessionRepository =
    abstract member Find: token:string -> Session option
    abstract member FindByEmail: email:string -> Session option
    abstract member Create: session:Session -> unit
    abstract member DeleteExpiredSessions: thresholdDate:DateTime -> unit

type ICompanyRepository = 
