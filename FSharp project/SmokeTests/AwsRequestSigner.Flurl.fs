module AWSRequestSignerFlurl

open System
open System.Runtime.CompilerServices
open Flurl.Http

[<Extension>]
type Extensions() =
    [<Extension>]
    static member Sign(request:IFlurlRequest, method:string, content:string option, service:string, region:string) = 
        let url = request.Url.Path

        let querystring = request.Url.Query
        let content = if content.IsSome then content.Value else ""

        if not(request.Headers.Contains("Host")) then
            request.Headers.Add("Host", request.Url.Host)

        let headers = request.Headers |> Seq.map (fun nv -> nv.ToTuple()) |> dict

        let generatedHeaders = AWSRequestSigner.createAuhtorizationHeaders(method, url, querystring, content, headers, service, region)
        request.WithHeaders(generatedHeaders)