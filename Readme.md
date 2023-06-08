# API generator for Genesys Cloud API

This is an experimental application to genereate a C# library to access Genesys Cloud API.

> **Important:** This code is NOT created or maintained by Genesys. 

At the moment it only generates a .NET project and source files.

To build the application clone the repo and run ```dotnet publish -c Release``` from the main directory.

To create the API project run the appliction with the following parameters:

- hostname: The host to download the swagger file (example: api.mypurecloud.de)
- target folder: The folder, where the project is created. The folder must exist.
- target namespace (optional): The namespace of the generated code. Default: GenesysCloud.Client.V2

Example:
```
ApiGenerator api.mypurecloud.de D:\MyProjects\GenesysApi MyNamespace
```

This would download the swagger from https://api.mypurecloud.de/api/v2/docs/swagger and generate the code with ```namespace MyNamespace``` in the directory ```D:\MyProjects\GenesysApi````.



