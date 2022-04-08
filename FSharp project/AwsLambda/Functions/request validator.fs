﻿module request_validator

open System
open Amazon.Lambda.APIGatewayEvents

let missingParameter parameter = $@"Parameter ""{parameter}"" not found in querystring"


type ValidationRule (parameterName:String) = 
    member this.Parameter with get() = parameterName

type IValidationRule =   
    abstract member Parameter: string
    abstract member Validate: APIGatewayProxyRequest -> Result<unit, string>
          
type ValidationError = { Parameter:string; Message:string}

let ValidateRequest (request:APIGatewayProxyRequest) (rules: IValidationRule list) =
    let checkRule rule =
        match (rule:IValidationRule).Validate request with
        | Ok _ -> None
        | Error error -> Some( { Parameter=rule.Parameter; Message = error })
    rules |> List.choose checkRule

type MustExistValidationRule (parameterName:string) =
    inherit ValidationRule(parameterName)

    interface IValidationRule with
        member this.Parameter: string = base.Parameter 
        member this.Validate request =
            match request.QueryStringParameters.ContainsKey parameterName with
            | true -> Ok ()
            | false -> Error (missingParameter parameterName)

let ParameterMustExist parameterName = MustExistValidationRule parameterName :> IValidationRule