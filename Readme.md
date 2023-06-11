# API generator for Genesys Cloud API

This is an experimental application to genereate a C# library to access Genesys Cloud API.

> **Important:** This code is NOT created or maintained by Genesys. 

The goal is to create a SDK with
- run on .NET6 an above
- use third party packages as less as possible
- use standard logging mechanism (Microsoft.Extensions.ILogger)
  
At the moment it only generates a .NET project and source files.

To build the application clone the repo and run ```dotnet publish -c Release``` from the main directory.

The generation of the API project needs 3 calls of the application with different parameters:

1. Run ```ApiGenerator swagger -h <hostname> -t <target folder> [ --namespace <ApiNamespace>]``` with:
- hostname: The host to download the swagger file (example: api.mypurecloud.de)
- target folder: The folder, where the project is created. The folder must exist.
- namespace (optional): The namespace of the generated code. Default: GenesysCloud.Client.V2

Example: ```ApiGenerator -h api.mypurecloud.de -t D:\MyProjects\GenesysApi --namespace MyNamespace ```

This will download the swagger from https://api.mypurecloud.de/api/v2/docs/swagger and generate the SDK code with ```namespace MyNamespace``` in the directory ```D:\MyProjects\GenesysApi```.

2. To get the type information of notification events are not part of the swagger file. To get this informaten run ```ApiGenerator readnotification --clientId <cliendId> --clientSecret  <clientSecret> --environment <environment>``` with:
- clientId: The client id for the request.
- clientSecret: The clientSecret for the request.
- environment: The environment to get the information from (Example: mypurecloud.de).

Example: ```ApiGenerator readnotification --clientId ************** --clientSecret  ******************** --environment mypurecloud.de ```

This will login with the given credentials get the list of available topics from the api and store them in ```notificationSchema.json``` in the current directory.

3. To generate the notification the code from the previous step run ```ApiGenerator notification -i <inputfile> -t <target folder> [ --namespace <ApiNamespace>]``` with:
- inputfile: The notification file created in the step above.
- target folder: The folder, where the project is created. 
- namespace (optional): The namespace of the generated code. Default: GenesysCloud.Client.V2

Example: ```ApiGenerator notification -i .\notificationSchema.json -t -t D:\MyProjects\GenesysApi --namespace MyNamespace ```

This will read the notification definitions and gerenate the SDK files in the target folder.

To generate the SDK run ```dotnet build -c Release``` from target folder.

