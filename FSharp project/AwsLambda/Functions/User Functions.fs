namespace Portfolio.Api.Functions

open System.Linq
open Amazon.Lambda.APIGatewayEvents
open Amazon.Lambda.Core
open Portfolio.Api.Functions
open Portfolio.Core
open Portfolio.Core.Entities
open Microsoft.Extensions.Configuration
open Portfolio.MongoRepository
open UserLogin
open SessionManager


type UserFunctions (repository:IUserRepository, sessionManager:ISessionManager) =
    inherit FunctionBase()

    new () =
        let configFile = "configuration.json"
        let variable = "MongoDB_connection_string"

        let configuration = ConfigurationBuilder()
                                .AddJsonFile(configFile)
                                .Build()

        let connectionString = configuration.[variable]
        if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."

        let sessionRepository = SessionRepository(connectionString)

        let sessionManager = SessionManager(sessionRepository)

        UserFunctions(UserRepository(connectionString), sessionManager)


    member this.Create (request:APIGatewayProxyRequest, context:ILambdaContext) =
        // TODO: do not log the request within sensitive data
        context.Logger.Log $"Create: {request.Body}"

        try
            let user:User = base.Deserialize request.Body
            let normalizedUser = user.Normalize()
            // TODO: validate

            // TODO: Mocked in unit tests but fails with Method Not Implemented Exception ?!
            let a = repository.Single(normalizedUser.Email)

            if repository.Single(normalizedUser.Email).IsSome then 
                this.createErrorForConflict "An user with this same email already exists."
            else
                repository.Create(normalizedUser)
                context.Logger.Log($"User {normalizedUser.Email} created")
                this.createCreated (normalizedUser.ObfuscatePassword())
        with exc ->
            context.Logger.Log $"Failed to create User. {exc}"
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
            this.createOk()

        with exc ->
            context.Logger.Log $"Failed to delete User. {exc}"
            this.createError $"Failed to delete User. {exc.Message}"

    member this.All (request:APIGatewayProxyRequest, context:ILambdaContext) =
        try
            let list = repository.All().Select (fun user -> user.ObfuscatePassword())
            this.createOkWithData list

        with exc ->
            context.Logger.Log $"Failed to retrieve users. {exc}"
            this.createError $"Failed to retrieve users. {exc.Message}"

    member this.Login (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"Login"
        try 
            let input:LoginInput = this.Deserialize request.Body

            match repository.Single input.Email with
            | None -> this.createResponse 200 (Some { IsSuccess=false; Error="User not found"; AuthToken=null})
            | Some user when user.IsBlocked -> this.createResponse 503 (Some"User is blocked")
            | Some user -> 
                let authToken = sessionManager.GetSession(user.Email).Token
                this.createResponse 200 (Some { IsSuccess=true; Error=null; AuthToken=authToken})

        with exc ->
            context.Logger.Log $"Failed to login. {exc}"
            this.createError $"Failed to login. {exc.Message}"

    member this.ClenupExpiredSessions (request:APIGatewayProxyRequest, context:ILambdaContext) =
        context.Logger.Log $"ClenupExpiredSessions"

        sessionManager.CleanupExpiredSessions()