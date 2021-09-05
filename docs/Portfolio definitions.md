# Portfolio definitions


## Currency
- Code
- Name

## Asset 
- Class: Cash, Cryptocurrency, Stock, Real estate
- Note: free text

## QuantityAtDate
- Date
- Quantity

## Provider
Entity that manage the asset.
- Name
- Type: Bank, Exchange, other

## Fund
- Provider
- Asset

  

Example
For "100 EUR on Fineco bank"

Fund:
  Provider: Fineco
  Asset:
    Currency: EUR
    CurrentQuantity: the quantity at the current date or 0 if not owned anymore
    Dates: array of QuantityAtDate

