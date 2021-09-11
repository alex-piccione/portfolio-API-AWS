module Portfolio.Api.Core.Entities

open System

type Currency = { Code:string; Name:string}

type User = {
    Username:string
    Email:string
    mutable Password:string
    PasswordHint:string
    CreatedOn:DateTime
    IsEmailValidated:bool
    IsBlocked:bool

    //member P = PAsswo
}