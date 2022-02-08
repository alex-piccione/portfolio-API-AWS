namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core
open error_messages


type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance
    abstract member CreateOrUpdate: request:BalanceUpdateRequest -> BalanceUpdateResult

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

        member this.CreateOrUpdate(request: BalanceUpdateRequest): BalanceUpdateResult = 

            let error = match request with 
                        | r when String.IsNullOrWhiteSpace r.CompanyId -> Some(mustBeDefined "CompanyId")
                        | _ -> None

            match error with
            | Some message -> BalanceUpdateResult.InvalidRequest message
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
                    BalanceUpdateResult.Updated
                | None -> 
                    fundRepository.CreateFundAtDate { record with Id = idGenerator.New() }
                    BalanceUpdateResult.Created