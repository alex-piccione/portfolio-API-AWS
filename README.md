# portfolio-API-AWS
AWS API for Portfolio project

## AWS


AWS Python lambda documentation: https://docs.aws.amazon.com/lambda/latest/dg/python-handler.html


### API GAteway

Create API Gateway "REST API" on eu-central-1 regios called "Portfolio".  
se Proxy.  

- GET ping
arn:aws:lambda:eu-central-1:151404309046:function:ping


## GitHub Actions
docs: https://docs.github.com/en/actions/reference/workflow-syntax-for-github-actions



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