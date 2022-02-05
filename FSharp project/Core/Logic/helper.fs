namespace Portfolio.Core.Logic

open System

type Result<'T> =
    | Ok of 'T
    | NotValid of string
    | Error of string

type IIdGenerator =
    abstract member New: unit -> string

type IdGenerator () = 
    interface IIdGenerator with
        member this.New () = Guid.NewGuid().ToString()

type IChronos =
    abstract member Now: DateTime

type Chronos () =
    interface IChronos with 
        member this.Now = DateTime.UtcNow