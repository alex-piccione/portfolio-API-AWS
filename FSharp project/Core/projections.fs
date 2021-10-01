module Portfolio.Core.Projections

open System
open Portfolio.Core.Entities


type BalanceListItem = {
    Currency:Currency
    Quantity: decimal
    Value: decimal
    // 
}

type Fund = {
    Currency: Currency
    Manager: Company
    Quantity: decimal
}

type FundAggregate = {
    Currency: Currency
    TotalQuantity: decimal
    Price: decimal
    TotalValue: decimal
    FundCompanies: string list
    //Managers: FundManager list
}

type Balance = {
    Date:DateTime
    AggregateFunds: FundAggregate list
}