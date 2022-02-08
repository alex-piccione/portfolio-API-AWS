module exceptions

open System

type InvalidRequestException (error:string) =
    inherit Exception(error)
