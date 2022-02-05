﻿namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core


type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance
    abstract member CreateOrUpdate: request:BalanceUpdateRequest -> BalanceUpdateResult


type BalanceLogic(fundRepository:IFundRepository, chronos:IChronos) = 

    interface IBalanceLogic with
        member this.GetBalance(day: DateTime): Balance = 
            let funds = fundRepository.GetFundsToDate(day)

            let fundsByCurrency = funds |> List.groupBy (fun fund -> fund.CurrencyCode)

            let aggregates = 
                fundsByCurrency |> List.map (fun (currencyCode, funds) -> 

                    let totalQuantity = funds.Sum(fun fund -> fund.Quantity)
                    let companies = funds |> List.filter (fun fund -> fund.Quantity > 0m) |> List.map (fun fund -> fund.FundCompanyId)

                    {
                        CurrencyCode = currencyCode
                        Quantity = totalQuantity
                        CompaniesIds = companies
                    }:FundForCurrency
                )
                |> List.filter (fun f -> f.Quantity > 0m)

            let balance:Balance = {Date=day; FundsByCurrency = aggregates }
            balance

        member this.CreateOrUpdate(request: BalanceUpdateRequest): BalanceUpdateResult = 
            
            let record:FundAtDate = { 
                Id = "" // doesn't matter
                Date = request.Date.Date
                CurrencyCode = request.CurrencyCode
                FundCompanyId = request.CompanyId
                Quantity = 0m // doesn't matter
                LastChangeDate = System.DateTime.MinValue // doesn't matter
            }

            match fundRepository.FindFundAtDate(record) with
            | Some existing ->
                fundRepository.UpdateFundAtDate { record with Id = existing.Id }
                    CurrencyCode = existing.CurrencyCode;
                    LastChangeDate = chronos.Now
                BalanceUpdateResult.Updated
            | None -> 
                fundRepository.CreateFundAtDate { record with Id = IdGenerator.NewId() }
                    Id = Guid.NewGuid().ToString()
                    Date = request.Date.Date
                    CurrencyCode = request.CurrencyCode
                    FundCompanyId = request.CompanyId
                    Quantity = request.Quantity
                    LastChangeDate = chronos.Now
                BalanceUpdateResult.Created