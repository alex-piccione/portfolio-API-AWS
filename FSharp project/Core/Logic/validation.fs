module validation
open System

let mustBeDefined field = $"{field} must be defined"
let mustBeGreaterThanZero field = $"{field} must be greater than zero"
let mustBeInTheFuture field = $"{field} must be in the future"
let mustBeInThePast field = $"{field} must be in the past"

let validate (checks: Result<_,string> list) =
   let checkRule check =
       match check with
       | Ok _ -> None
       | Error error -> Some error
   checks |> List.choose checkRule

let stringIsNotEmpty property value = 
    if String.IsNullOrWhiteSpace(value) then Error (mustBeDefined property)
    else Ok ()
     
let dateIsDefined property value = 
    if value = Unchecked.defaultof<DateTime> then Error (mustBeDefined property)
    else Ok ()

let dateIsInThePast property now value =
    if value < now then Ok ()
    else Error (mustBeInThePast property)

let inline numberIsPositive property value = 
    if value > LanguagePrimitives.GenericZero then Ok()
    else Error (mustBeGreaterThanZero property)
 