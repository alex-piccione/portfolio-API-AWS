namespace Portfolio.Api.Functions

open System
open Microsoft.Extensions.Configuration
open Amazon.Lambda.Core
open Amazon.Lambda.APIGatewayEvents
open Portfolio.Api.Functions
open Portfolio.MongoRepository
open Portfolio.Core.Logic


type BalanceFunctions (balanceLogic:IBalanceLogic) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        BalanceFunctions(BalanceLogic(FundRepository(connectionString)))


    member this.Get (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Get Balance. {request.Body}"

        // TODO: creae a Validate function that accept many checks as a list and pass over all of them exiting at the fist one and returning a DU OK or the Fail
        let baseCurrencyCode:string option = this.GetValueFromQuerystring request "base-currency"
        if baseCurrencyCode.IsNone then base.createErrorForMissingQuerystring "base-currency"

        else
            let balance = balanceLogic.GetBalance(DateTime.UtcNow.Date)
            base.createOkWithData balance