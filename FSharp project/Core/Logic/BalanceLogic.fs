namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core
open error_messages

type BalanceUpdateResult = 
| Created
| Updated

type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance
    abstract member CreateOrUpdate: request:BalanceUpdateRequest -> Result<BalanceUpdateResult, string>
    abstract member GetFundOfCurrencyByDate: currencyCode:string * minDate:DateTime -> CurrencyFundAtDate list

type BalanceLogic(fundRepository:IFundRepository, chronos:IChronos, idGenerator:IIdGenerator) = 

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
            match request with 
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

            let groupCompanies (funds:FundAtDate list):CompanyFund list = 
                funds
                |> List.map(fun f -> {Id=f.Id; CompanyId=f.FundCompanyId; Quantity=f.Quantity; LastUpdateDate=f.LastChangeDate })

            operations 
                |> List.groupBy(fun f  -> f.Date)
                |> List.map(fun g -> { Date=fst(g); CompanyFunds=groupCompanies (snd(g)); TotalQuantity=0m })