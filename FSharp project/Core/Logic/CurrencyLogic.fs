namespace Portfolio.Core.Logic

open System
open Portfolio.Core
open Portfolio.Core.Entities

type ICurrencyLogic =
    abstract member Create:Currency -> Result<Currency, string>
    abstract member Update:Currency -> Result<Currency, string>
    abstract member Single:string -> Currency option
    abstract member All:unit -> Currency list
    //abstract member Delete:string -> Result<unit,string>

type CurrencyLogic(currencyRepository:ICurrencyRepository, fundRepository:IFundRepository) =
    
    //let assignNewCode currency:Currency = {currency with Id=Guid.NewGuid().ToString()}

    let normalize currency:Currency = {currency with Code=currency.Code.Trim(); Name=currency.Name.Trim()}
      
    let isEmpty value = String.IsNullOrWhiteSpace value

    let validate currency = 
        match currency with
        | c when isEmpty currency.Code -> Error "Code cannot be empty."
        | c when isEmpty currency.Name -> Error "Name cannot be empty."
        | _ -> Ok currency

    let checkNameExists (currency:Currency) = 
        match currencyRepository.ExistsWithName currency.Name with 
        | true -> Error $"A currency with name \"{currency.Name}\" already exists."
        | _ -> Ok currency

    interface ICurrencyLogic with
        member this.Create(currency:Currency) =
            match normalize currency |> validate with
            | Error msg -> Error msg
            | Ok validCurrency ->
                match currencyRepository.ExistsWithCode validCurrency.Code with 
                | true -> Error $"A currency with code \"{validCurrency.Code}\" already exists."
                | _ -> 
                    match checkNameExists validCurrency with
                    | Error msg -> Error msg
                    | Ok _ -> currencyRepository.Create(validCurrency); Ok validCurrency
         
        member this.Single code = currencyRepository.Single code
        member this.All () = List.ofSeq (currencyRepository.All ())

        member this.Update (currency:Currency) =        
            match normalize currency |> validate with
            | Error msg -> Error msg
            | Ok validCurrency -> 
                match checkNameExists validCurrency with
                | Error msg -> Error msg
                | Ok _ -> currencyRepository.Update validCurrency; Ok validCurrency
                
        (*member this.Delete id =
            match (fundRepository.GetFundsOfCurrency id).IsEmpty with
            | true -> 
                companyRepository.Delete id
                Ok ()
            | _ -> Error $"Company \"{id}\" is used in funds."
            *)