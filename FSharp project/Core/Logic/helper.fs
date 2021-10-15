namespace Portfolio.Core.Logic

type Result<'T> =
    | Ok of 'T
    | NotValid of string
    | Error of string

