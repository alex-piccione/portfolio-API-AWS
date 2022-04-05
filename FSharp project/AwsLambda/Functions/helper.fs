module helper

open Microsoft.Extensions.Configuration


let private getConnectionString () =
    let configFile = "configuration.json"
    let variable = "MongoDB_connection_string"

    let configuration = ConfigurationBuilder()
                            .AddJsonFile(configFile)
                            .Build()
    let connectionString = configuration.[variable]
    if connectionString = null then failwith $@"Cannot find ""{variable}"" in ""{configFile}""."
    connectionString

let connectionString = Lazy<string>.Create(fun () -> getConnectionString())
let ConnectionString = connectionString.Value
