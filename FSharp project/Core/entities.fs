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

type CompanyTypesJsonConverter () =
    inherit JsonConverter<CompanyType list>()

    override this.Read(reader, typeToConvert, options) =
        let mutable values = List.Empty
        while reader.Read() && reader.TokenType <> JsonTokenType.EndArray do
            match reader.TokenType with 
            | JsonTokenType.String -> 
                values <- CompanyType.Parse(reader.GetString())::values
            | _ -> ()
        values

    override this.Write(writer, value, options) =
        writer.WriteStartArray()
        value |> List.iter (fun item -> writer.WriteStringValue (item.ToString())) 
        writer.WriteEndArray()

type Company = {
    Id: string
    Name: string
    [<JsonConverter(typeof<CompanyTypesJsonConverter>)>]
    Types: CompanyType list
}



// it's very tricky to find a good name for the Exchanges/Banks avoiding "Manager", "Controller", "Handler" and "Admin".
// Examples: Custodian, Holder, Producer, Organizer, Governor, Provider, Keeper
(*type FundKeeper = {
    Company: Company
}*)

type FundAtDate = {
    Id: string
    Date: DateTime
    CurrencyCode: string
    FundCompanyId: string
    Quantity: decimal
    LastChangeDate: DateTime
}

type CurrencyFundAtDate = {
    Date: DateTime
    CompanyFunds: CompanyFund list
    TotalQuantity: decimal
} 
and CompanyFund = {
    //IsInherited: bool
    Id: string option
    CompanyId: string    
    Quantity: decimal    
    LastUpdateDate: DateTime
}

type Balance = {
    Date: DateTime
    FundsByCurrency: FundForCurrency list
    LastUpdateDate: DateTime
}
and FundForCurrency = {
    CurrencyCode: string
    Quantity: decimal
    CompaniesIds: string list
    LastUpdateDate: DateTime
}

type BalanceUpdateRequest = {
    Date: DateTime
    CurrencyCode: string
    Quantity: decimal
    CompanyId: string
}