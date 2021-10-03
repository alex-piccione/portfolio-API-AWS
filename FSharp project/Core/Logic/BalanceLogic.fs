namespace Portfolio.Core.Logic

open System
open System.Linq
open Portfolio.Core.Projections
open Portfolio.Core.Entities
open Portfolio.Core


type IBalanceLogic =
    abstract member GetBalance: date:DateTime -> Balance


type BalanceLogic(fundRepository:IFundRepository, currencyRepository:ICurrencyRepository) = 

    interface IBalanceLogic with
        member this.GetBalance(day: DateTime): Balance = 
            let funds = fundRepository.GetFundsAtDate(day)
            let currencies = currencyRepository.All().ToDictionary(fun kv -> kv.Code)

            let fundsByCurrency = funds |> List.groupBy (fun fund -> fund.CurrencyCode)

            let aggregates = 
                fundsByCurrency |> List.map (fun (currencyCode, funds) -> 
                    let found, currency = currencies.TryGetValue(currencyCode)
                    if not found then failwith $"Currency \"{currencyCode}\" does not exists."

                    let totalQuantity = funds.Sum(fun fund -> fund.Quantity)
                    let companies = funds |> List.map (fun fund -> fund.FundCompanyId)

                    { 
                        Currency=currency
                        TotalQuantity=totalQuantity
                        TotalValue=1m
                        Price=1m
                        FundCompanies=companies
                    }:FundAggregate
                )

            let balance:Balance = {Date=day; AggregateFunds = aggregates }
            balance

