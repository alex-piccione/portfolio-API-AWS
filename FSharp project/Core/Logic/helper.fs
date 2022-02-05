namespace Portfolio.Core.Logic

type Result<'T> =
    | Ok of 'T
    | NotValid of string
    | Error of string

type IdGenerator () = 
    static member NewId () = System.Guid.NewGuid().ToString()

type IChronos =
    abstract member Now: System.DateTime

type Chronos () =
    interface IChronos with 
        member this.Now = System.DateTime.UtcNow