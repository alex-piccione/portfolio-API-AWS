namespace Portfolio.Api.Functions

open System
open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Portfolio.Api.Functions
open Portfolio.MongoRepository
open Portfolio.Core.Logic
open Portfolio.Core.Entities
open request_validator

type BalanceFunctions (balanceLogic:IBalanceLogic) =
    inherit FunctionBase()

    new () =
        BalanceFunctions(BalanceLogic(FundRepository(helper.ConnectionString), Chronos(), IdGenerator()))

    member this.Get (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Get Balance. {request.Body}"

        let errors = ValidateRequest request [ParameterMustExist "base-currency"]
        if errors.IsEmpty then
            let balance = balanceLogic.GetBalance(DateTime.UtcNow.Date)
            base.createOkWithData balance        
        else base.createErrorForInvalidRequest errors      

    member this.Update (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Update Balance. {request.Body}"

        try
            let updateRequest = base.Deserialize<BalanceUpdateRequest> request.Body
            match balanceLogic.CreateOrUpdate(updateRequest) with
            | Ok Created -> this.createCreated ()
            | Ok Updated -> this.createOk ()
            | Error error -> this.createErrorForConflict error
        with exc ->
            context.Logger.Log $"Failed to update Balance. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to update Balance. Error: {exc.Message}"
