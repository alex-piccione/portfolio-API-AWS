module secrets

open Microsoft.Extensions.Configuration
open System


let configuration =
    ConfigurationBuilder()
        .AddUserSecrets("AWS.Portfolio")
        .Build()

let loadSecrets path env =
    match configuration.[path] with
    | null -> 
        match Environment.GetEnvironmentVariable(env) with 
        | null -> failwith $"""Secret with path "{path}" and Environment variable "{env}" are null."""
        | value ->  value
    | value ->  value

let url =       loadSecrets "AWS:url" "URL"
let host =      loadSecrets "AWS:host" "HOST"
let accessKey = loadSecrets "AWS:access key" "KEY"
let secretKey = loadSecrets "AWS:secret key" "SECRET"