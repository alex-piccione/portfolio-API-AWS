namespace AwsLambda

open Amazon.Lambda.Core

[<assembly: LambdaSerializer(typeof<Amazon.Lambda.Serialization.Json.JsonSerializer>)>]
do ()

module Say =
    let hello name =
        printfn "Hello %s" name
