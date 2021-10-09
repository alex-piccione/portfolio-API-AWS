module Portfolio.Core.Entities

open System
open System.Text.Json.Serialization
open System.Text.Json

type User = {
    Username:string
    Email:string
    Password:string
    PasswordHint:string
    CreatedOn:DateTime
    IsEmailValidated:bool
    IsBlocked:bool
    } with
    member this.ObfuscatePassword () = { this with Password = "***" }
    member this.Normalize () = 
        { this with 
            Username = this.Username.Trim()
            Email = this.Email.Trim().ToLowerInvariant()
            Password = this.Password.Trim().ToLowerInvariant()
        }

type Session = {
    Email:string
    Token:string
    CreatedOn:DateTime
    ExpireOn:DateTime
}

type Currency = { Code:string; Name:string}


type CompanyType =
    | Bank
    | Exchange
    | Stacking

    static member Parse value = 
        match value with 
        | "Bank" -> CompanyType.Bank
        | "Exchange" -> CompanyType.Exchange
        | "Stacking" -> CompanyType.Stacking
        | _ -> failwith $"\"{value}\" is not a valid CompanyType."


type Company = {
    Id: string
    Name: string
    Types: CompanyType list
}


// it's very difficult to find a name for the Exchanges/Banks avoiding "Manager", "Controller", "Handler" and "Admin".
// Custodian, Holder, Producer, Organizer, Governor, Provider, Custodian. Keeper
(*type FundKeeper = {
    Company: Company
}*)

type FundAtDate = {
    Date: DateTime
    CurrencyCode: string
    FundCompanyId: string
    Quantity: decimal
}
