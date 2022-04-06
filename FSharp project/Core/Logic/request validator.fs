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




type Validator (checks:List<Check>) =
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