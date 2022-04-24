module validator
open System

type IValidationCheck =   
    abstract member Validate: unit -> Result<unit, string>

type StringIsNotEmptyCheck (property:string, value:string) =
    interface IValidationCheck with
        member this.Validate () =
            match String.IsNullOrWhiteSpace(value) with
            | false -> Ok ()
            | _ -> Error (error_messages.mustBeDefined property)

let StringIsNotEmpty property value = StringIsNotEmptyCheck(property, value)
     
type DateIsDefinedCheck (property:string, value:DateTime) =
     interface IValidationCheck with
         member this.Validate () =
             match value <> Unchecked.defaultof<DateTime> with
             | true -> Ok ()
             | _ -> Error (error_messages.mustBeDefined property)

let DateIsDefined property value = DateIsDefinedCheck(property, value)

type DateIsInThePastCheck (property:string, value:DateTime, now:DateTime) =
    interface IValidationCheck with
        member this.Validate () =
            match value < now with
            | true -> Ok ()
            | _ -> Error (error_messages.mustBeInThePast property)

let DateIsInThePast property now value = DateIsInThePastCheck(property, value, now)

type DecimalIsPositiveCheck (property:string, value) =
    interface IValidationCheck with
        member this.Validate () = 
            match value > 0m with
            | true -> Ok()
            | _ -> Error (error_messages.mustBeGreaterThanZero property)

let DecimalIsPositive property value = DecimalIsPositiveCheck(property, value)

          
let Validate (checks: IValidationCheck list) =
   let checkRule check =
       match (check:IValidationCheck).Validate () with
       | Ok _ -> None
       | Error error -> Some error
   checks |> List.choose checkRule
