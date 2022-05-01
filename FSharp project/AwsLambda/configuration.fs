module configuration

open Microsoft.Extensions.Configuration
open Portfolio.MongoRepository

type Configuration = {ConnectionString:string; Database:string; Counter:int}
    
let mutable counter = 0

let loadConfiguration () = 
    counter <- counter + 1
    let conf = ConfigurationBuilder()
                   .AddJsonFile("configuration.json")
                   .AddEnvironmentVariables("Portfolio:") // used by Emulator
                   .Build()    
    let database = conf["MongoDB_database"]
    {
        ConnectionString = conf["MongoDB_connection_string"]
        Database = if database = null then "Portfolio" else database
        Counter = counter
    }

let databaseConfig:DatabaseConfig = { 
    ConnectionString = (lazy(loadConfiguration())).Value.ConnectionString
    Database = (lazy(loadConfiguration())).Value.Database
    }
