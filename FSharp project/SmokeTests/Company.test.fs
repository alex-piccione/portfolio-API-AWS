module SmokeTests.Company

open NUnit.Framework
open FsUnit
open Flurl.Http
open AWSRequestSignerFlurl


[<Test>]
let ``Company All`` () =
    let response = 
        $"https://{secrets.url}/company/all"
            .AllowAnyHttpStatus()
            .Sign("GET", None, "execute-api", "eu-central-1")
            .GetAsync().Result

    let content = response.GetStringAsync().Result

    if response.StatusCode <> 200
    then
        Assert.Fail(content)
    else
        content |> should not' (be NullOrEmptyString)


[<Test>]
let ``Company All (no auth)`` () =
    let response = 
        $"https://{secrets.url}/company/all"
            .AllowAnyHttpStatus()
            //.WithHeader("Host", secrets.host)
            .GetAsync().Result

    let content = response.GetStringAsync().Result

    if response.StatusCode <> 200
    then
        Assert.Fail(content)
    else
        content |> should not' (be NullOrEmptyString)
