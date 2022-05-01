module configuration

open Microsoft.Extensions.Configuration

let configuration =
    ConfigurationBuilder()
        .AddUserSecrets("d118f5b8-a2c9-418a-9068-fc37358467bd")
        .AddEnvironmentVariables()
        .Build()

let loadSecrets path =
    match configuration.[path] with
    | null -> failwith $"""Secret with path "{path}" is null."""
    | value -> value

let connectionString = loadSecrets "MongoDB:connection string"
