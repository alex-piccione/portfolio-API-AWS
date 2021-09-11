namespace Portfolio.Api.Core

open Portfolio.Api.Core.Entities
open System.Collections.Generic

type CRUD<'T> =
    abstract member Create: 'T -> unit
    abstract member Single: string -> 'T
    abstract member Update: 'T -> string -> unit
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
