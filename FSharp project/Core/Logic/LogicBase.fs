namespace Portfolio.Core.Logic

open System


type LogicBase () = 

    //member mustBeDefined field
    member this.isDateUndefined date = date = Unchecked.defaultof<DateTime>
    //member this.isDateInTheFuture date = date = Unchecked.defaultof<DateTime>
