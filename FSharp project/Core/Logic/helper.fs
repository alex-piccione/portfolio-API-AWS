namespace Portfolio.Core.Logic

type Result<'T> =
    | Ok of 'T
    | NotValid of string
    | Error of string

type IIdGenerator =
    abstract member New: unit -> string

type IdGenerator () = 
    interface IIdGenerator with
        member this.New () = System.Guid.NewGuid().ToString()

type IChronos =
    abstract member Now: System.DateTime

type Chronos () =
    interface IChronos with 
        member this.Now = System.DateTime.UtcNow