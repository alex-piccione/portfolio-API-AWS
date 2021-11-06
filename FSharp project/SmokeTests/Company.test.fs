module SmokeTests.Company

open System
open NUnit.Framework
open FsUnit
open Flurl.Http
open AWSRequestSignerFlurl
open System.Text.Json
open Portfolio.Core.Entities


[<Test>]
let ``All`` () =
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
let ``All without AWS signature`` () =
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

[<Test>]
let ``Single`` () =


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
let ``Create & Delete`` () =
    
    let create () =
        let data:Company = {Id=""; Name="TEST-3"; Types=[CompanyType.Bank]}
        let response = 
            $"https://{secrets.url}/company"
                .AllowAnyHttpStatus()
                .PostJsonAsync(data).Result

        let content = response.GetStringAsync().Result

        if response.StatusCode <> 201
        then
            Assert.Fail($"Create failed. {content}")
            ""
        else
            content |> should not' (be NullOrEmptyString)
            let data = JsonSerializer.Deserialize<Company>(content)
            data.Id |> should not' (be NullOrEmptyString)
            data.Id

    let delete id =
        let response = 
            $"https://{secrets.url}/company/{id}"
                .AllowAnyHttpStatus()
                //.WithHeader("Host", secrets.host)
                .Sign("DELETE", None, "execute-api", "eu-central-1")
                .DeleteAsync().Result

        let content = response.GetStringAsync().Result

        if response.StatusCode <> 200
        then
            Assert.Fail($"Delete failed. {content}")
        else
            content |> should not' (be NullOrEmptyString)


    let id = create()
    delete id