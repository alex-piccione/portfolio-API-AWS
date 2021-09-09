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

        context.Logger.Log($"request.QueryStringParameters: {request.QueryStringParameters}")

        if request.QueryStringParameters = null then this.createError("Missing querystring")
        else
            match request.QueryStringParameters.TryGetValue("code") with
            | (true, id) -> this.createOkWithData(this.Repository.Single id)
            | _ -> failwith @"Missing querystring parameter ""code""."


[<Class>]
type All () = 
    inherit CurrencyFunction()

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        context.Logger.Log("List")

        let list = [
            { Code="EUR"; Name="Euro"}
            { Code="XRP"; Name="Ripple"}
        ]

        base.createOkWithData(Some(list))


[<Class>]
type Create (repository:ICurrencyRepository) = 
    inherit CurrencyFunction(repository)

    member this.Handle (request:APIGatewayProxyRequest, context:ILambdaContext) = 

        // TODO: validate
        // TODO: store in repository

        this.createOkWithStatus (201)