namespace Portfolio.Api.Functions

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
                this.createCreated (user.ObfuscatePassword())
        with exc ->
            context.Logger.Log $"Failed to create User. Data: {user.ObfuscatePassword()}. Error: {exc}"
            this.createError $"Failed to create User. {exc.Message}"

    member this.Single (request:APIGatewayProxyRequest, context:ILambdaContext) =

        let found, email = request.PathParameters.TryGetValue "email"
        if not found then failwith $"Path should contain \"email\"."

        try
            match repository.Single (email.ToLowerInvariant()) with
            | Some user -> this.createResponse 200 (Some user)
            | _ -> this.createResponse 404 None

        with exc ->
            context.Logger.Log $"Failed to read User. {exc}"
            this.createError $"Failed to read User. {exc.Message}"

    member this.Delete (request:APIGatewayProxyRequest, context:ILambdaContext) =

        let found, email = request.PathParameters.TryGetValue "email"
        if not found then failwith $"Path should contain \"email\"."

        try
            repository.Delete (email.ToLowerInvariant()) 
            this.createResponse 204 None

        with exc ->
            context.Logger.Log $"Failed to delete User. {exc}"
            this.createError $"Failed to delete User. {exc.Message}"

    member this.All (request:APIGatewayProxyRequest, context:ILambdaContext) =
        repository.All().Select (fun user -> user.ObfuscatePassword())