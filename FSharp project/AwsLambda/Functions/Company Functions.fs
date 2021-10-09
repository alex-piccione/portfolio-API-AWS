namespace Portfolio.Api.Functions

open System
open Microsoft.Extensions.Configuration
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities


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

        // TODO: validate request
        // fields not null
        // no duplicated Name

        try
            let item = {base.Deserialize request.Body with Id=Guid.NewGuid().ToString()}
            
            repository.Create(item)
            context.Logger.Log $"Company created. New Id:{item.Id}, Name:{item.Name}"
            this.createOkWithStatus 201
        with exc ->
            context.Logger.Log $"Failed to create Company. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Company. Error: {exc.Message}"

    member this.Single (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"request.QueryStringParameters: {request.QueryStringParameters}"

        if request.QueryStringParameters = null then this.createError("Missing querystring")
        else
            match request.QueryStringParameters.TryGetValue("id") with
            | (true, id) -> 
                match repository.Single id with
                | Some item -> base.createOkWithData item
                | _ -> base.createNotFound()
            | _ -> failwith @"Missing querystring parameter ""id""."

    member this.Update (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Update Company"

        let itemUpdate:Company = base.Deserialize request.Body

        let item = repository.Single (itemUpdate.Id)

        match item.IsNone with
        | true -> base.createNotFound()
        | false -> 
            try 
                repository.Update itemUpdate
                base.createOk()
            with exc ->
                context.Logger.Log $"Failed to update Company. {exc}"
                this.createError $"Failed to update Company. {exc.Message}"


    member this.Delete (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Delete Company"

        let found, id = request.PathParameters.TryGetValue "id"
        if not found then failwith @"Path should contain ""id""."

        try
            repository.Delete id
            this.createOk()
        with exc ->
            context.Logger.Log $"Failed to delete Company. {exc}"
            this.createError $"Failed to delete Company. {exc.Message}"

