namespace Portfolio.Api.Functions

open System.Text
open Amazon.Lambda.APIGatewayEvents
open Microsoft.Extensions.Configuration
open Portfolio.Api.MongoRepository
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

[<AbstractClass>]
type CurrencyFunction (repository:ICurrencyRepository) =
    inherit FunctionBase()

    member this.Repository = repository

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        let repository = CurrencyRepository(connectionString)
        CurrencyFunction(repository)