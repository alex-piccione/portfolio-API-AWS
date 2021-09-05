import json
from entities import Currency
from uuid import uuid4

def handle_create(event, context):

    id = uuid4()

    return {
        "statusCode": 200,
        "headers": { "Content-Type": "application/json"},
        "body": {id}
    }


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

