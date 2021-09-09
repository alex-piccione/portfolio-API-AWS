namespace Portfolio.Api.Core

open Portfolio.Api.Core.Entities

type CRUD<'T> =
    abstract member Create: 'T -> unit
    abstract member Read: string -> 'T
    abstract member Update: 'T * string -> unit
    abstract member Delete: string -> unit

type All<'T> =
    abstract member All: unit -> 'T list

type IUserRepository =
    inherit CRUD<User>
    inherit All<User>

