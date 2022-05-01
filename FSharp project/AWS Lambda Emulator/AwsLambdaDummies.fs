module AwsLambdaDummies

open Amazon.Lambda.Core
open System

type LambdaLogger () =
    interface ILambdaLogger with
        member this.Log(message: string): unit = Console.WriteLine message
         
        member this.Log(level: string, message: string): unit = 
            raise (System.NotImplementedException())
        member this.Log(level: LogLevel, message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogCritical(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogDebug(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogError(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogInformation(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogLine(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogTrace(message: string): unit = 
            raise (System.NotImplementedException())
        member this.LogWarning(message: string): unit = 
            raise (System.NotImplementedException()) 

type LambdaContext (functionName:string, logger:ILambdaLogger) = 
    interface ILambdaContext with
        member this.AwsRequestId: string = 
            raise (System.NotImplementedException())
        member this.ClientContext: IClientContext = 
            raise (System.NotImplementedException())
        member this.FunctionName: string = functionName
        member this.FunctionVersion: string = 
            raise (System.NotImplementedException())
        member this.Identity: ICognitoIdentity = 
            raise (System.NotImplementedException())
        member this.InvokedFunctionArn: string = 
            raise (System.NotImplementedException())
        member this.LogGroupName: string = 
            raise (System.NotImplementedException())
        member this.LogStreamName: string = 
            raise (System.NotImplementedException())
        member this.Logger: ILambdaLogger = logger
        member this.MemoryLimitInMB: int = 
            raise (System.NotImplementedException())
        member this.RemainingTime: TimeSpan = 
            raise (System.NotImplementedException())