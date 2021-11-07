module Balance

open System

type UpdateBalanceRequest = {
    Date: DateTime
    CurrencyCode: string
    Quantity: decimal
    CompanyId: string
}

