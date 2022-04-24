//namespace Portfolio.Core.Logic
module validator
open System

(*
let isDateUndefined date = date = Unchecked.defaultof<DateTime>

type LogicResult = { IsSuccess:bool; Errors:string list }
type RuleResult = { Pass:bool; Error:string }
type Rule<'request> = 'request -> Option<string> // option

type ToCheck = 
    | Value of obj
    | Func of (unit -> obj)

type Check =
    | CheckDate of (DateTime * string -> string option)
    | CheckString of (string * string -> string option)

let checkDateIsDefined date field:Check = CheckDate(fun _ ->
    match date <> Unchecked.defaultof<DateTime> with
    | true -> None
    | _ -> Some(error_messages.mustBeDefined field))

let checkDateIsInTheFuture date field:Check = CheckDate(fun _ ->
    match date > DateTime.UtcNow with
    | true -> None
    | _ -> Some(error_messages.mustBeDefined field))

let checkStringIsDefined value field:Check = CheckString(fun _ ->
    match String.IsNullOrWhiteSpace(value) with
    | true -> Some(error_messages.mustBeDefined field)
    | _ -> None)
    *)





//type ValidationCheck (parameterName:String) = 
//    member this.Parameter with get() = parameterName

type IValidationCheck =   
    //abstract member Parameter: string
    abstract member Validate: unit -> Result<unit, string>


let inline divideByTwo value = 
    LanguagePrimitives.DivideByInt value 2

//divideByTwo 1 |> ignore  
divideByTwo 1f |> ignore
divideByTwo 1m |> ignore

type Calculator () =
    member this.DivideByTwo value = 
        LanguagePrimitives.DivideByInt value 2

//let half = Calculator().DivideByTwo(1) // DivideByInt does not support int !!

// cannot use both the following, the first one will "force" the type, and the other will not work
let a_half = Calculator().DivideByTwo(1f) // ok if used before the "decimal" version
//let b_half = Calculator().DivideByTwo(1m) // ok only if comment the previous one


type number =
| I of int 
| D of decimal 

type Checker () =
    member this.Validate value =
        match value with 
        | I x when x > 0 -> "ok"
        | D x when x > 0m -> "ok"
        | _ -> "error"

    member this.ValidateGeneric value =
        match LanguagePrimitives.GenericGreaterThan value 0m with
        | true -> "ok"
        | _ -> "error"

let a = 1f
let b = 1m
//let a_IsValid = Checker().Validate(a) // does not compile, expect number (not int)
//let b_IsValid = Checker().Validate(b) // does not compile, expect number (not decimal)
//let a_isValid = Checker().ValidateGeneric(a)

let b_isValid = Checker().ValidateGeneric(b)



type NumberIsPositiveCheck (property:string, value) =
    interface IValidationCheck with
        member this.Validate () = 
            match LanguagePrimitives.GenericGreaterThan value 0 with
            | true -> Ok()
            | _ -> Error (error_messages.mustBeGreaterThanZero property)
            // match value with
            //| I x when x > 0 -> Ok()
            //| D x when x > 0m -> Ok()
            //| _ -> Error (error_messages.mustBeGreaterThanZero property)

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






type FieldValidationResult = { FieldName:string; IsValid:bool; Errors:string list }


let validate (rules:string option list) = rules |> List.choose (fun e -> e)

let validate_ rules =
    rules |> List.filter (fun v -> not v.IsValid) 
    |> List.map (fun v -> v.Errors)


    (*
let validate (item:'request, rules:Rule<'request> list) =
    let checkRule rule = match rule.check item with
                         | true, _ -> None
                         | _, error -> Some(error)
    let errors = rules |> List.choose checkRule
    if errors.IsEmpty then pass() else fails errors
    *)

//let fails error = {IsSuccess=False}

//type Validator<'R> () = 
//    static member mustBeDefined field:LogicResult = error_messages.mustBeDefined(field)