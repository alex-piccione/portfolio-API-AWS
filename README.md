# Portfolio API on AWS
AWS API for Portfolio project

![Deploy F# API](https://github.com/alex-piccione/portfolio-API-AWS/actions/workflows/deploy%20fsharp.yml/badge.svg)


## AWS
AWS Python lambda documentation: https://docs.aws.amazon.com/lambda/latest/dg/python-handler.html

IAM user: portfolio.lambda (Portfolio.API group)
Required permissions:
- AmazonS3FullAccess
- AWSCloudFormationFullAccess
- AWSLAmbdaRole
- AmazonAPIGatewayInvokeFullAccess
- AmazonAPIGatewayAdministrator
- AWSLambdaBasicExecutionRole


### API GAteway

Create API Gateway "REST API" on eu-central-1 regios called "Portfolio".  


## GitHub Actions
docs: https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions

## Serverless
docs: ???

GitHub action I'm using seems to be abandoned'  
``deploy`` command stopped to work within the --config parameter.  
I use it to load the configuration grom another file, how to fo it withou the parameter?  
The error says the configuration must stay in the service root folder (working directory), so moving the file will be enough?  
Documentation is not easy to find. For example searching "deploy" in the servh box does not go where you want.  
To find it I had to search on google: https://www.serverless.com/framework/docs/providers/aws/cli-reference/deploy/  


## Configuration
The __configuration.json__ file inside "AWS Lambda" project contains info that cannot be stored in the code repository.  
During the deployment a precise step replaces defined keys with the secrets stored in the deploy system (e.g. GitHub Actions).  

## Dependency Injection
I don't know the best way to make possible having dependency injection, for example to inject database providers.  
I saw some solutions but it seems overcomplicated.  
To solve the problem of having a parameterless constructor for the Lambda Functions and inject the mocked providers for test purpose
I'm using a default empty contructor and also one that accepts the provider(s) interface.  
When the parameterless contructor is used the code look at a configuration file and create the provider using.  


## MongoDB

Access granted to user with permission ``readWrite`` on ``Portfolio`` database.  
GitHub secrets:
- MONGODB_USER
- MONGODB_PASSWORD

Connection example:
```
const MongoClient = require('mongodb').MongoClient;
const uri = "mongodb+srv://<username>:<password>@cluster0.rxzmw.gcp.mongodb.net/myFirstDatabase?retryWrites=true&w=majority";
const client = new MongoClient(uri, { useNewUrlParser: true, useUnifiedTopology: true });
client.connect(err => {
  const collection = client.db("test").collection("devices");
  // perform actions on the collection object
  client.close();
});
```


### Add a new property
When a new property is added to the type stored in the database it makes the deserialization fails.  
This bug is not revealed by the tests because they creates new records that results to be ok within the change.  
When the code is deployed and used over the old records it raise the error.  
Set a default value in the MongoDB Collection mapping solves the issue.  
The ``static member overloadMap`` function can be used:
```fsharp
static member overloadMap (map:BsonClassMap<FundAtDate>) = 
   map.MapProperty(fun f -> f.LastChangeDate).SetDefaultValue(DateTime(2000, 01, 01)) |> ignore
```

Don't forget to pass the overload method in the constructor.
