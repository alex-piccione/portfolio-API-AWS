namespace Portfolio.Api.Core

open Portfolio.Api.Core.Entities

type ICurrencyRepository =

    abstract member Get: string -> Currency
    abstract member Create: Currency -> unit
    abstract member List: unit -> Currency list

