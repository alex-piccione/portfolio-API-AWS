module Portfolio.Core.Entities

open System

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


type Company = {
    Id: string
    Name: string
    Types: CompanyType list
}


type FundAdministrator = {
    Company: Company
}

type FundAtDate = {
    Date: DateTime
    CurrencyCode: string
    FundCompanyId: string
    Quantity: decimal
}

(*
type Fund = {
    Currency: Currency
    Manager: Company
    Quantity: decimal
}

type FundAggregate = {
    Currency: Currency
    TotalQuantity: decimal
    TotalValue: decimal
    Managers: FundAdministrator list
}

type Balance = {
    Date:DateTime
    AggregateFunds: FundAggregate list
}
*)