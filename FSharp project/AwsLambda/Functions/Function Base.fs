namespace Portfolio.Api.Functions

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
    //let jsonOptions = Options.ISO8601CamelCase;

    //static let connectionString = Lazy<string>.Create(fun () -> helper.getConnectionString())
    //static let GetConnectionString() = connectionString.Value

    (*
    static member getConnectionString () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()
        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."
        connectionString
        *)

    //member internal this.ConnectionString with get() = GetConnectionString()

    member this.createResponse<'T> statusCode (data:'T option): APIGatewayProxyResponse =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- statusCode
        response.Headers <- dict["Content-Type", "application/json"]
        response.Body <-
            match data with
            | None -> ""
            | Some x -> Json.JsonSerializer.Serialize(x)
        response

    member this.createOk () =
        this.createResponse 200 None

    member this.createOkWithStatus statusCode =
        this.createResponse statusCode None

    member this.createOkWithData<'T> data =
        this.createResponse<'T> 200 (Some data)

    member this.createCreated<'T> data =
        this.createResponse<'T> 201 (Some data)

    member this.createNotFound () =
        this.createResponse<string> 404 (Some "")

    member this.createError message =
        this.createResponse<string> 500 (Some message)

    member this.createErrorForConflict message =
        this.createResponse<string> 409 (Some message)

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