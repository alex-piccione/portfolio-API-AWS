import json

def handle(event, context):

    return {
        "statusCode": 200,
        "headers": { "Content-Type": "text"},
        "body": "pong"
    }