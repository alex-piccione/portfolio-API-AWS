namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Microsoft.Extensions.Configuration
open Portfolio.MongoRepository


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

    member private this.single id =
        try 
            match repository.Single id with
            | Some item -> this.createOkWithData item
            | _ -> base.createNotFound()
        with exc ->
            failwith $"Failed to get Single. {exc}"


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"
        try
            let currency = base.Deserialize request.Body
            repository.Create(currency)
            this.createOkWithStatus 201
        with exc ->
            context.Logger.Log $"Failed to create Currency. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Currency. Error: {exc.Message}"

    
    member this.All (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log("All")
        try
            let list = repository.All()
            base.createOkWithData(list)
        with exc ->
            context.Logger.Log $"Failed to retrieve Currencies. {exc}"
            this.createError $"Failed to retrieve Currencies. Error: {exc.Message}"

    member this.Single (request:APIGatewayProxyRequest, context:ILambdaContext) =

        context.Logger.Log($"request.QueryStringParameters: {request.QueryStringParameters}")

        if request.QueryStringParameters = null then this.createError("Missing querystring")
        else
            match request.QueryStringParameters.TryGetValue("code") with
            | (true, id) -> this.single id
            | _ -> failwith @"Missing querystring parameter ""code""."