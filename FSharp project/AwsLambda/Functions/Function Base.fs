﻿namespace Portfolio.Api.Functions

open System
open System.Text
open System.Text.Json
open Microsoft.Extensions.Configuration
open Amazon.Lambda.APIGatewayEvents
open Portfolio.Core.Entities

type DataOrOption<'T> =
    | Option of Option<'T>
    | Data of 'T

type FunctionBase () =
    
    member this.createResponse statusCode (data:obj option): APIGatewayProxyResponse =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- statusCode
        match data with 
        | None -> 
            response.Body <- ""
            response.Headers <- dict<string, string>["Content-Type", "text/plain"]
        | Some value -> 
            if value :? string 
            then 
                response.Headers <- dict<string, string>["Content-Type", "text/plain"]
                response.Body <- value :?> string 
            else 
                response.Headers <- dict<string, string>["Content-Type", "application/json"]
                response.Body <- Json.JsonSerializer.Serialize(value)
        response

    member this.createOk () = this.createResponse 200 None

    member this.createOkWithData data = this.createResponse 200 (Some data)

    member this.createCreated data = this.createResponse 201 (Some data)

    member this.createNotFound () = this.createResponse 404 None

    member this.createError (message:string) = this.createResponse 500 (Some message)

    member this.createErrorForConflict (message:string) = this.createResponse 409 (Some message)

    member this.createErrorForMissingQuerystring missingParameter =
        this.createErrorForConflict $"\"{missingParameter}\" parameter is missing in the querystring."


    member this.Deserialize<'T>(requestBody:string) =
        if String.IsNullOrEmpty requestBody then failwith $"Request body is empty"

        let options = JsonSerializerOptions()
        options.PropertyNameCaseInsensitive <- true
        options.Converters.Add(CompanyTypesJsonConverter())

        try JsonSerializer.Deserialize<'T>(requestBody, options)
        with e -> failwith $"Failed to deserialize request body to {typeof<'T>}. {e}"

    member this.GetValueFromQuerystring (request:APIGatewayProxyRequest) (property:string) =
        match request.QueryStringParameters with
        | null -> None
        | _ -> match request.QueryStringParameters.TryGetValue property with
               | true, value -> Some(value)
               | _ -> None