module validator_spike
open System

// TODO: waiting answer on StackOverflow: https://stackoverflow.com/questions/71991828/f-greaterthanzero-passing-int-or-decimal


type IValidationCheck =   
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

          
let Validate (checks: IValidationCheck list) =
   let checkRule check =
       match (check:IValidationCheck).Validate () with
       | Ok _ -> None
       | Error error -> Some error
   checks |> List.choose checkRule

