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
        FundFunctions(BalanceLogic(FundRepository(helper.ConnectionString), Chronos(), IdGenerator()))

    member this.GetFund (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Get Fund. {request.Body}"

        let errors = ValidateRequest request [ParameterMustExist "currency"; ParameterMustExist "from"]
        if errors.IsEmpty then
            let currency = this.GetValueFromQuerystring request "currency" 
            let minDate = this.GetDateFromQuerystring request "from" 
            match (currency, minDate) with
            //| None, None -> 
            | None, _ -> base.createErrorForConflict (emptyStringParameter "currency")
            | _, None -> base.createErrorForConflict (emptyStringParameter "currency")
            | Some currencuCode, Some date  -> base.createOkWithData (balanceLogic.GetFund (currencuCode, date))
            //match this.GetValueFromQuerystring request "currency" with            
            //| Some currencyCode ->        
            //     let minDate = this.GetDateFromQuerystring request "from"  
            //     base.createOkWithData (balanceLogic.GetFund (currencyCode, minDate))
            //| None -> base.createErrorForInvalidRequest errors              
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
