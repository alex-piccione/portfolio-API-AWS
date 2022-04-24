//namespace Portfolio.Core.Logic
module validator
open System

let isDateUndefined date = date = Unchecked.defaultof<DateTime>

type LogicResult = { IsSuccess:bool; Errors:string list }
type RuleResult = { Pass:bool; Error:string }
type Rule<'request> = 'request -> Option<string> // option

//type Check<'request> = 'request * ('request -> obj) -> Option<string> 
//type Check = (unit -> obj) -> Option<string> 


type ToCheck = 
    | Value of obj
    | Func of (unit -> obj)

type Check =
    | CheckDate of (DateTime * string -> string option)
    | CheckString of (string * string -> string option)

//type Rule = string Check

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
     
 type DateIsDefinedCheck (property:string, value:DateTime) =
     interface IValidationCheck with
         member this.Validate () =
             match value <> Unchecked.defaultof<DateTime> with
             | true -> Ok ()
             | _ -> Error (error_messages.mustBeDefined property)

let DateIsDefinedCheck property value = DateIsDefinedCheck(property, value)

type DateIsInThePastCheck (property:string, value:DateTime, now:DateTime) =
    interface IValidationCheck with
        member this.Validate () =
            match now > value with
            | true -> Ok ()
            | _ -> Error (error_messages.mustBeInThePast property)

type DecimalIsPositiveCheck (property:string, value) =
    interface IValidationCheck with
        member this.Validate () = 
            match value > 0m with
            | true -> Ok()
            | _ -> Error (error_messages.mustBeGreaterThanZero property)

let DecimalIsPositiveCheck property value = DecimalIsPositiveCheck(property, value)

          
type ValidationError = { Message:string}

let Validate (checks: IValidationCheck list) =
   let checkRule check =
       match (check:IValidationCheck).Validate () with
       | Ok _ -> None
       | Error error -> Some error
   checks |> List.choose checkRule























type _Validator (checks:List<Check>) =
    member this.validate () = 
        let run = fun check -> 
            match check with 
            | CheckDate d -> d(DateTime.Now, "")
            | _ -> None
            
        checks |> List.choose run //(fun check -> check  )
//type Rulea<'request> = { check: 'request -> bool * string}


// ref: https://stackoverflow.com/a/15092057/996946
// TODO: put in a module to enable pipe syntax
//type Checks with 
//    member this.validate () = this |> List.choose (fun c -> c)

let pass () = { IsSuccess=true; Errors=List.Empty }
let fails (errors:string list) = { IsSuccess=false; Errors=errors }


let checkDateIsDefined__ getData fieldName = 
    match getData() = Unchecked.defaultof<DateTime> with 
    | true -> false, $"{fieldName} must be defined"
    | _ -> true, ""

type FieldValidationResult = { FieldName:string; IsValid:bool; Errors:string list }

let checkDateIsDefined_ value fieldName = 
    match value = Unchecked.defaultof<DateTime> with 
    | true -> Some $"{fieldName} must be defined"
    | _ -> None

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