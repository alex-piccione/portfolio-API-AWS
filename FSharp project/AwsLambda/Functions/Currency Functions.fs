namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Portfolio.MongoRepository


type CurrencyFunctions (repository:ICurrencyRepository) =
    inherit FunctionBase()

    new () =
        CurrencyFunctions(CurrencyRepository(base.ConnectionString))

    member private this.single id =
        try 
            match repository.Single id with
            | Some item -> this.createOkWithData item
            | _ -> base.createNotFound()
        with exc ->
            failwith $"Failed to get Single. {exc}"

    member private this.all () =
        try
            let list = repository.All()
            base.createOkWithData(list)
        with exc ->
            this.createError $"Failed to retrieve Currencies. Error: {exc.Message}"


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"
        try
            let currency = base.Deserialize request.Body
            repository.Create(currency)
            this.createOkWithStatus 201
        with exc ->
            context.Logger.Log $"Failed to create Currency. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Currency. Error: {exc.Message}"


    member this.Read (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log($"request.QueryStringParameters: {request.QueryStringParameters}")

        if request.QueryStringParameters = null 
        then this.all()
        else 
            match request.QueryStringParameters.TryGetValue("code") with
            | (true, id) -> this.single id
            | _ -> failwith @"Missing querystring parameter ""code""."