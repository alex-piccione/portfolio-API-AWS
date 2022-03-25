namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICurrencyLogic =
    abstract member Create:Currency -> Result<Currency, string>
    //abstract member Update:Company -> Result<Company, string> // UpdateResult
    abstract member Single:string -> Currency option
    abstract member List:unit -> Currency list
    //abstract member Delete:string -> Result<unit,string>  // DeleteResult

type CurrencyValidation = Valid of Currency | NotValid of string

type CurrencyLogic(currencyRepository:ICurrencyRepository, fundRepository:IFundRepository) =
    
    //let assignNewCode currency:Currency = {currency with Id=Guid.NewGuid().ToString()}

    let normalize currency:Currency = {currency with Code=currency.Code.Trim(); Name=currency.Name.Trim()}

    let isEmpty value = String.IsNullOrWhiteSpace value

    let validate currency = 
        match currency with
        | c when isEmpty currency.Code -> CurrencyValidation.NotValid "Code cannot be empty."
        | c when isEmpty currency.Name -> CurrencyValidation.NotValid "Name cannot be empty."
        | _ -> CurrencyValidation.Valid currency

    interface ICurrencyLogic with
        member this.Create(currency:Currency) =
            match normalize currency |> validate with
            | NotValid msg -> Error msg
            | Valid validCurrency ->
                match currencyRepository.ExistsWithCode validCurrency.Code with 
                | true -> Error $"A currency with code \"{validCurrency.Code}\" already exists."
                | _ -> 
                    match currencyRepository.ExistsWithName validCurrency.Name with 
                    | true -> Error $"A currency with name \"{validCurrency.Name}\" already exists."
                    | _ -> 
                        currencyRepository.Create(validCurrency)
                        Ok validCurrency
         
        member this.Single code = currencyRepository.Single code
        member this.List () = List.ofSeq (currencyRepository.All ())