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

            //type x (date:DateTime, funds:FundAtDate list) list

            //let addInherited (data:(date:DateTime, items:FundAtDate list) list) =
            let addInherited (byDate: CurrencyFundAtDate list) = // byDate:(DateTime * FundAtDate list) list =

                let add (index:int) (atDate:CurrencyFundAtDate) = 
                    //let date = fst(data)
                    //let funds = snd(data)
                    if index > 0 then
                        // exists record before ?
                        let previous:CurrencyFundAtDate = byDate[index-1]                        
                        for x:CompanyFund in previous.CompanyFunds do                        
                            match atDate.CompanyFunds |> List.tryFind (fun f -> f.CompanyId = x.CompanyId) with
                            | Some d -> ()
                            | None -> atDate.CompanyFunds.Append {x with Id = None} |> ignore
                        ()
                    else
                        //  search for info before the date 0 ???
                        ()
                    ()

                byDate |> List.iteri add

                byDate

            let groupCompanies (funds:FundAtDate list):CompanyFund list = 
                funds
                |> List.map(fun f -> {Id=Some(f.Id); CompanyId=f.FundCompanyId; Quantity=f.Quantity; LastUpdateDate=f.LastChangeDate })

            let sum (funds:FundAtDate list) = funds |> List.fold (fun acc f -> acc+f.Quantity) 0m

            operations 
                |> List.groupBy(fun f -> f.Date)                              
                |> List.map(fun (date:DateTime, funds:FundAtDate list) -> { Date=date; CompanyFunds=groupCompanies funds; TotalQuantity=sum funds })
                |> List.sortBy(fun x -> x.Date)   
                |> addInherited