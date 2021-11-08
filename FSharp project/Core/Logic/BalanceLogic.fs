namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Entities
open Portfolio.Core


type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance
    abstract member Update: request:BalanceUpdateRequest -> BalanceUpdateResult


type BalanceLogic(fundRepository:IFundRepository) = 

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

        member this.Update(request: BalanceUpdateRequest): BalanceUpdateResult = 
            
            let record:FundAtDate = { 
                Id=""
                Date=request.Date.Date
                CurrencyCode= request.CurrencyCode
                FundCompanyId= request.CompanyId
                Quantity= request.Quantity
            }

            match fundRepository.FindFundAtDate(record) with
            | Some existing -> 
                fundRepository.UpdateFundAtDate record 
                BalanceUpdateResult.Updated
            | None -> 
                fundRepository.CreateFundAtDate record
                BalanceUpdateResult.Created

            