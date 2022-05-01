namespace Portfolio.Api.Functions

open System
open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Portfolio.Api.Functions
open Portfolio.MongoRepository
open Portfolio.Core.Logic
open Portfolio.Core.Entities
open request_validator

type FundFunctions (balanceLogic:IBalanceLogic) =
    inherit FunctionBase()

    new () =
        FundFunctions(BalanceLogic(FundRepository(configuration.databaseConfig), Chronos(), IdGenerator()))

    member this.GetFund (request:APIGatewayProxyRequest, context:ILambdaContext) =
        base.Log(context, "Get Fund", request.Body)

        let errors = ValidateRequest request [ParameterMustExist "currency"; ParameterMustExist "from"]
        if errors.IsEmpty then
            let currency = this.GetValueFromQuerystring request "currency" 
            let minDate = this.GetDateFromQuerystring request "from" 
            match (currency, minDate) with
            | None, _ -> base.createErrorForConflict (emptyStringParameter "currency")
            | _, None -> base.createErrorForConflict (invalidDateParameter "from" minDate)
            | Some currencuCode, Some minDate  -> base.createOkWithData (balanceLogic.GetFundOfCurrencyByDate (currencuCode, minDate))
        else base.createErrorForInvalidRequest errors

    member this.Update (request:APIGatewayProxyRequest, context:ILambdaContext) =
        base.Log(context, "Update", request.Body)

        try
            let updateRequest = base.Deserialize<BalanceUpdateRequest> request.Body
            match balanceLogic.CreateOrUpdate(updateRequest) with
            | Ok Created -> this.createCreated ()
            | Ok Updated -> this.createOk ()
            | Error error -> this.createErrorForConflict error
        with exc ->
            context.Logger.Log $"Failed to update Balance. Data: {request.Body}. Error: {exc}"
            this.createError $"Failed to update Balance. Error: {exc.Message}"
