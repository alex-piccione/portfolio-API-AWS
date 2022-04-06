namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.MongoRepository
open Portfolio.Core.Logic


type CurrencyFunctions (currencyLogic:ICurrencyLogic) =
    inherit FunctionBase()

    new () =                
        CurrencyFunctions(
            CurrencyLogic(
                CurrencyRepository(helper.ConnectionString), 
                FundRepository(helper.ConnectionString)))

    member private this.single id =
        try 
            match currencyLogic.Single id with
            | Some item -> this.createOkWithData item
            | _ -> base.createNotFound()
        with exc ->
            failwith $"Failed to get Single. {exc}"

    member private this.all () =
        try
            let list = currencyLogic.All()
            base.createOkWithData(list)
        with exc ->
            this.createError $"Failed to retrieve Currencies. Error: {exc.Message}"


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"

        try
            let currency = base.Deserialize request.Body
            match currencyLogic.Create currency with
            | Ok created -> this.createCreated created
            | Error message -> this.createErrorForConflict message
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