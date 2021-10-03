namespace Portfolio.Api.Functions

open Microsoft.Extensions.Configuration
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Portfolio.MongoRepository



type CompanyFunctions (repository:ICompanyRepository) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        CompanyFunctions(CompanyRepository(connectionString))


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"
        try
            let currency = base.Deserialize request.Body
            repository.Create(currency)
            this.createOkWithStatus 201
        with exc ->
            context.Logger.Log $"Failed to create Currency. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Currency. Error: {exc.Message}"


    member this.Single (request:APIGatewayProxyRequest, context:ILambdaContext) =

        context.Logger.Log($"request.QueryStringParameters: {request.QueryStringParameters}")

        if request.QueryStringParameters = null then this.createError("Missing querystring")
        else
            match request.QueryStringParameters.TryGetValue("code") with
            | (true, id) -> base.createOkWithData (repository.Single id)
            | _ -> failwith @"Missing querystring parameter ""code""."