﻿namespace Portfolio.Api.Functions

open System.Linq
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
        // TODO: do not log the request within sensitive data
        context.Logger.Log $"Create: {request.Body}"
        let user = base.Deserialize request.Body
        
        // TODO: validate and normalize input

        try
            if repository.Single(user.Email).IsSome then 
                this.createError "An user with this same email already exists."
            else
                repository.Create(user)
                context.Logger.Log($"User {user.Email} created")
                this.createCreated (Some(user.ObfuscatePassword()))
        with exc ->
            context.Logger.Log $"Failed to create User. Data: {user.ObfuscatePassword()}. Error: {exc}"
            this.createError $"Failed to create User. {exc.Message}"

    member this.All (request:APIGatewayProxyRequest, context:ILambdaContext) =
        repository.All().Select (fun user -> user.ObfuscatePassword())