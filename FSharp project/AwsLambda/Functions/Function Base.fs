namespace Portfolio.Lambda.Functions

open System.Text
open Amazon.Lambda.APIGatewayEvents
open Portfolio.Api.Core


type FunctionBase () =

    //let jsonOptions = Options.ISO8601CamelCase;

    member this.createOk<'T> (data:'T option) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 200
        response.Headers <- dict["Content-Type", "application/json"]
        response.Body <-
            match data with
            | None -> ""
            | Some x -> Json.JsonSerializer.Serialize(x)
        response

    member this.createError (message:string) =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 500
        response.Body <- message
        //response.Headers <- dict["Content-Type", "application/json"]
        response

