﻿namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core
open error_messages
open validator


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

            [
                validator.checkDateExists (fun _ -> request.Date) "Date"
                validator.checkFieldIsDefined (fun _ -> request.CompanyId) "Date"
            ]

            //let rule:Rule<BalanceUpdateRequest> = validator.mustBeDefined("Date" , fun r -> r.Date)
            let rules = [
                checkDateIsDefined (fun _ -> request.Date)
                //request.Date = Unchecked.defaultof<DateTime> -> mustBeDefined "Date"
            ]
            let result = validate(request, rules)

            let invalidRequest error = BalanceUpdateResult.InvalidRequest error

            match request with 
            | r when r.Date = Unchecked.defaultof<DateTime> -> invalidRequest <| mustBeDefined "Date"
            | r when r.Date = Unchecked.defaultof<DateTime> -> invalidRequest (mustBeDefined "Date")
            | r when String.IsNullOrWhiteSpace r.CompanyId -> invalidRequest (mustBeDefined "CompanyId")
            | r when r.Quantity <= 0m -> invalidRequest (mustBeGreaterThanZero "Quantity")
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