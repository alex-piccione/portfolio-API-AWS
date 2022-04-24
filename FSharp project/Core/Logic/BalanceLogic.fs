namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core
open error_messages
open validator

type BalanceUpdateResult = 
| Created
| Updated

type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance
    abstract member CreateOrUpdate: request:BalanceUpdateRequest -> Result<BalanceUpdateResult, string>
    abstract member GetFundOfCurrencyByDate: currencyCode:string * minDate:DateTime -> CurrencyFundAtDate list

type BalanceLogic(fundRepository:IFundRepository, chronos:IChronos, idGenerator:IIdGenerator) = 
    inherit LogicBase()

    interface IBalanceLogic with
        member this.GetBalance(day: DateTime): Balance = 
            let funds = fundRepository.GetFundsToDate(day)
            let fundsByCurrency = funds |> List.groupBy (fun fund -> fund.CurrencyCode)
            let mutable balanceLastUpdate = DateTime.MinValue

            let aggregates = 
                fundsByCurrency |> List.map (fun (currencyCode, funds) -> 

                    let totalQuantity = funds.Sum(fun fund -> fund.Quantity)
                    let companies = funds |> List.filter (fun fund -> fund.Quantity > 0m) |> List.map (fun fund -> fund.FundCompanyId)
                    let lastUpdateDate = funds.Max(fun fund -> fund.LastChangeDate)
                    balanceLastUpdate <- max balanceLastUpdate lastUpdateDate 

                    {
                        CurrencyCode = currencyCode
                        Quantity = totalQuantity
                        CompaniesIds = companies
                        LastUpdateDate = lastUpdateDate
                    }:FundForCurrency
                )
                |> List.filter (fun f -> f.Quantity > 0m)

            let balance:Balance = {Date=day; FundsByCurrency = aggregates; LastUpdateDate = balanceLastUpdate }
            balance

        member this.CreateOrUpdate(request: BalanceUpdateRequest) =
            //let errors = validate [
            //    checkDateIsDefined_ request.Date "Date"
            //    checkDateIsInTheFuture request.Date "Date"
            //]
            //if not errors.IsEmpty Error errors.Head
            //else

            match request with 
            | r when base.isDateUndefined r.Date -> Error <| mustBeDefined "Date"
            | r when r.Date = Unchecked.defaultof<DateTime> -> Error <| mustBeDefined "Date"
            | r when r.Date > chronos.Now -> Error <| mustBeInThePast "Date"
            | r when String.IsNullOrWhiteSpace r.CurrencyCode -> Error (mustBeDefined "CurrencyCode")
            | r when String.IsNullOrWhiteSpace r.CompanyId -> Error (mustBeDefined "CompanyId")
            | r when r.Quantity <= 0m -> Error (mustBeGreaterThanZero "Quantity")
            | _ ->
                let record:FundAtDate = { 
                    Id = "" // to be set
                    Date = request.Date.Date
                    CurrencyCode = request.CurrencyCode
                    FundCompanyId = request.CompanyId
                    Quantity = request.Quantity
                    LastChangeDate = chronos.Now
                }

                match fundRepository.FindFundAtDate(record) with
                | Some existing ->
                    fundRepository.UpdateFundAtDate { record with Id = existing.Id }
                    Ok Updated
                | None -> 
                    fundRepository.CreateFundAtDate { record with Id = idGenerator.New() }
                    Ok Created

        member this.GetFundOfCurrencyByDate(currencyCode: string, minDate: DateTime): CurrencyFundAtDate list = 
            let operations = fundRepository.GetFundsOfCurrency(currencyCode, minDate)

            let addInherited (initial:CurrencyFundAtDate list) (current:CurrencyFundAtDate) =
                match initial.IsEmpty with
                | true -> current::initial
                | _ ->
                    let renewedFunds = 
                        initial.Head.CompanyFunds
                        |> List.filter (fun x -> x.Quantity > 0m) // remove Currency set to 0
                        |> List.fold (
                            fun (acc: CompanyFund list) prev -> 
                                match current.CompanyFunds |> List.tryFind (fun curr -> curr.CompanyId = prev.CompanyId) with
                                | None -> {prev with Id = None}::acc // add inherited
                                | _ -> acc
                                ) current.CompanyFunds   

                    let next = {current with 
                                    CompanyFunds=renewedFunds |> List.sortBy(fun c -> c.CompanyId); 
                                    TotalQuantity=renewedFunds |> List.sumBy(fun x -> x.Quantity)}

                    next::initial

            let groupCompanies (funds:FundAtDate list):CompanyFund list = 
                funds
                |> List.map(fun f -> {Id=Some(f.Id); CompanyId=f.FundCompanyId; Quantity=f.Quantity; LastUpdateDate=f.LastChangeDate })

            let sum (funds:FundAtDate list) = funds |> List.fold (fun acc f -> acc+f.Quantity) 0m

            operations 
                |> List.groupBy(fun f -> f.Date)                              
                |> List.map(fun (date:DateTime, funds:FundAtDate list) -> { Date=date; CompanyFunds=groupCompanies funds; TotalQuantity=sum funds })
                |> List.sortBy(fun x -> x.Date)  
                |> List.fold addInherited List.Empty