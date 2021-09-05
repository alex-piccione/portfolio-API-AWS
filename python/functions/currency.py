import json
from python.entities import Currency
import currency

def handle_list(event, context):

    eur = Currency("EUR", "Euro")
    xrp = Currency("XRP", "Ripple", 8)

    currencies = [
        "EUR",
        "GBP",
        "XRP",
        "STORJ"
    ]

    return {
        "statusCode": 200,
        "headers": { "Content-Type": "application/json"},
        "body": {currencies}
    }