# portfolio-API-AWS
AWS API for Portfolio project



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