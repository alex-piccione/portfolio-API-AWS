namespace Portfolio.Api.Functions

open System
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.MongoRepository
open Portfolio.Core.Entities
open Portfolio.Core.Logic

type CompanyFunctions (companyLogic:ICompanyLogic) =
    inherit FunctionBase()

    new () =
        CompanyFunctions(CompanyLogic(CompanyRepository(helper.ConnectionString), FundRepository(helper.ConnectionString)))

    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Create: {request.Body}"

        try
            let result = companyLogic.Create (base.Deserialize request.Body)
            match result with 
            | Ok newItem -> 
                context.Logger.Log $"Company created. New Id:{newItem.Id}, Name:{newItem.Name}."
                this.createCreated newItem
            | Error error -> base.createErrorForConflict error
        with exc ->
            context.Logger.Log $"Failed to create Company. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to create Company. Error: {exc.Message}"


    member this.Read (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"request.QueryStringParameters: {request.QueryStringParameters}"

        match request.QueryStringParameters with
        | null -> this.createOkWithData (companyLogic.All())
        | _ -> match request.QueryStringParameters.TryGetValue("id") with
               | (true, id) ->
                    match companyLogic.Single id with 
                    | Some item -> this.createOkWithData item
                    | _ -> this.createNotFound()                                
               | _ -> this.createErrorForConflict $"\"id\" parameters is the only one accepted."


    member this.Update (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Update Company"
        
        let itemToUpdate:Company = base.Deserialize request.Body

        let item = companyLogic.Single itemToUpdate.Id

        match item.IsNone with
        | true -> base.createNotFound()
        | false -> 
            try 
                let result = companyLogic.Update itemToUpdate
                match result with 
                | Ok updatedItem -> 
                    context.Logger.Log $"Company created. New Id:{updatedItem.Id}, Name:{updatedItem.Name}."
                    this.createOk()
                | Error error -> base.createErrorForConflict error
            with exc ->
                context.Logger.Log $"Failed to update Company. {exc}"
                this.createError $"Failed to update Company. {exc.Message}"


    member this.Delete (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Delete Company"

        let found, id = request.PathParameters.TryGetValue "id"
        if not found then failwith @"Path should contain ""id""."

        try
            match companyLogic.Delete id with
            | Ok _ -> this.createOk()
            | Error error -> this.createErrorForConflict error
        with exc ->
            context.Logger.Log $"Failed to delete Company. {exc}"
            this.createError $"Failed to delete Company. {exc.Message}"