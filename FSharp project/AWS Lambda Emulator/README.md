# AWS Lambda Emulator
This project is a web application that emulate the AWS Lambda Functions.  
Once started the Functions will be available on localhost:{choosen port} so it will be possible to test them locally.  

URL is defined by the Program.fs file.  
A _configuration.json_ file is required in order to connect to the database.  
It will overrides the one in the "AWS Lambda" project.  
Maybe in future it will be usefull to run it in a Docker container.  

## Variables
https://www.dowdandassociates.com/blog/content/howto-set-an-environment-variable-in-windows-command-line-and-registry/
To set a variable:
CMD: ``setx name "value of the variable"``
``setx MongoDB_connection_string "..."``
with the prefix:
``setx Portfolio:MongoDB_connection_string "..."``

setx Portfolio:MongoDB_connection_string "mongodb+srv://PortfolioAdmin:Port...1042@cluster0.74mtl.mongodb.net/myFirstDatabase?retryWrites=true&w=majority"