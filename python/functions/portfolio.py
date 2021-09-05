import json

def lambda_handler(event, context):

    return {
        "statusCode": 200,
        "headers": { "Content-Type": "text"},
        "body": "portfolio"
    }