namespace Portfolio.Core.Logic

type Result<'T> =
    | Ok of 'T
    | NotValid of string
    | Error of string

type IdGenerator () = 
    static member NewId () = System.Guid.NewGuid().ToString()
