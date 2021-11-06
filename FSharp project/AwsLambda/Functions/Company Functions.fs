namespace Portfolio.Api.Functions

open System
open Microsoft.Extensions.Configuration
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open Portfolio.Core.Logic


type CompanyFunctions (companyLogic:ICompanyLogic, repository:ICompanyRepository) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        CompanyFunctions(CompanyLogic(CompanyRepository(connectionString)), CompanyRepository(connectionString))




    member private this.single id =
        try 
            match repository.Single id with
            | Some item -> this.createOkWithData item
            | _ -> base.createNotFound()
        with exc ->
            failwith $"Failed to get Single. {exc}"

    member private this.all () =
        try
            base.createOkWithData (repository.All())
        with exc ->
            failwith $"Failed to call All. {exc}"


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"

        try
            let result = companyLogic.Create (base.Deserialize request.Body)
            match result with 
            | Ok newItem -> 
                context.Logger.Log $"Company created. New Id:{newItem.Id}, Name:{newItem.Name}."
                this.createCreated newItem
            | NotValid error -> base.createErrorForConflict error
            | Error error -> base.createError error
        with exc ->
            context.Logger.Log $"Failed to create Company. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Company. Error: {exc.Message}"


    member this.Read (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"request.QueryStringParameters: {request.QueryStringParameters}"

        match request.QueryStringParameters with
        | null -> this.all()
        | _ -> match request.QueryStringParameters.TryGetValue("id") with
               | (true, id) -> this.single id
               | _ -> this.createErrorForConflict $"\"id\" parameters is the only one accepted."


    member this.Update (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Update Company"

        let itemToUpdate:Company = base.Deserialize request.Body

        let item = repository.Single (itemToUpdate.Id)

        match item.IsNone with
        | true -> base.createNotFound()
        | false -> 
            try 
                let result = companyLogic.Update itemToUpdate
                match result with 
                | Ok updatedItem -> 
                    context.Logger.Log $"Company created. New Id:{updatedItem.Id}, Name:{updatedItem.Name}."
                    this.createOk()
                | NotValid error -> base.createErrorForConflict error
                | Error error -> base.createError error
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