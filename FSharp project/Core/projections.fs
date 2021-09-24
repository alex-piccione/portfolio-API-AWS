namespace Portfolio.Core.Projections

open Portfolio.Api.Core.Entities


type BalanceListItem = {
    Currency:Currency
    Quantity: decimal
    Value: decimal
    // 
}