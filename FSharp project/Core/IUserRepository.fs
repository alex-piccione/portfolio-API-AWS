namespace Portfolio.Api.Core

open Portfolio.Api.Core.Entities

type CRUD<'T> =
    abstract member Create: 'T -> unit
    abstract member Delete: string -> unit
    abstract member Update: 'T * string -> unit
    abstract member Get: string -> 'T
    abstract member All: unit -> 'T list

type IUserRepository =
    inherit CRUD<User>


