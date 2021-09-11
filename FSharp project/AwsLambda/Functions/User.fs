﻿namespace Portfolio.Api.Functions

open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Api.Core
open Portfolio.Api.Core.Entities
open Microsoft.Extensions.Configuration
open Portfolio.Api.MongoRepository


type UserFunctions (repository:IUserRepository) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        UserFunctions(UserRepository(connectionString))


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =

        context.Logger.Log $"Create: {request}"

        let user = base.Deserialize request.Body

        try
            repository.Create(user)
            context.Logger.Log($"User {user.Email} created")
        with exc ->
            user.Password <- "***"
            context.Logger.Log $"Failed to create User. Data: {user}. Error: {exc}"

    member this.All (request:APIGatewayProxyRequest, context:ILambdaContext) =
        // TODO: obfuscate passwords
        repository.All()