namespace Portfolio.Api.Functions.Currency

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open Microsoft.Extensions.Configuration
open Portfolio.Api.MongoRepository


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

[<Class>]
type Get () =
    inherit CurrencyFunction()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) =
        match request.QueryStringParameters.TryGetValue("code") with
        | (true, id) -> this.Repository.Get id
        | _ -> failwith @"Missing querystring parameter ""code""."


[<Class>]
type List (repository:ICurrencyRepository) = 
    inherit FunctionBase()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        let list = [
            { Code="EUR"; Name="Euro"}
            { Code="XRP"; Name="Ripple"}
        ]

        base.createOk(Some(list))


[<Class>]
type Create () = 

    let createOk () =
        let response = APIGatewayProxyResponse()
        response.StatusCode <- 204

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        // TODO: validate
        // TODO: store in repository

        createOk()