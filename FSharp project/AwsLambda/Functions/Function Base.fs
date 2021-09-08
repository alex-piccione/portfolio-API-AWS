namespace Portfolio.Api.Functions

open System.Text
open Amazon.Lambda.APIGatewayEvents
open Microsoft.Extensions.Configuration

type FunctionBase () =

    //let jsonOptions = Options.ISO8601CamelCase;

    member this.createResponse<'T> (statusCode:int, data:'T option) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- statusCode
        response.Headers <- dict["Content-Type", "application/json"]
        response.Body <-
            match data with
            | None -> ""
            | Some x -> Json.JsonSerializer.Serialize(x)
        response

    member this.createOk () =
        this.createResponse(200, None)

    member this.createOkWithStatus (statusCode:int) =
        this.createResponse(statusCode, None)

    member this.createOkWithData<'T> (data:'T) =
        this.createResponse(200, Some data)

    member this.createCreated<'T> (data:'T option) =
        this.createResponse(201, data)

    member this.createError (message:string) =
        this.createResponse(500, Some(message))

