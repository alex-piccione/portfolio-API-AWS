namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open Microsoft.Extensions.Configuration
open Portfolio.Api.MongoRepository


type CurrencyFunctions (repository:ICurrencyRepository) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        CurrencyFunctions(CurrencyRepository(connectionString))


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =

        context.Logger.Log $"Create: {request}"

        let code = "aaa"
        let name = "name"

        let currency = { Code = code; Name = name }
        try
            repository.Create(currency)
        with exc ->
            context.Logger.Log $"Failed to create Currency. Data: {currency}. Error: {exc}"