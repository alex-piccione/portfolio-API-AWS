import json

def handle(event, context):

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