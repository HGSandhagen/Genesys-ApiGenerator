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

## Usage

### Console app

To use the api in a simple console app:
1. Create an instance of ```GenesysCloudCredentials``` which holds the client id, client secret and the environment for the api access.
2. Create an instance of ```ConnectionManager```.
3. Create an instance of the API you want to use.
4. Use the api.
 
> **_Attention:_**  You should not have credential values in your code. This is for demonstartion only. In your application read it from a secure place (e.g. KeyVault).

Example:
```csharp
GenesysCloudCredentials credentials = new GenesysCloudCredentials() {
    ClientId = "<Enter your client id here>",
    ClientSecret = "<Enter your client secret here>",
    Environment = "<The environment of your organisation>" // e.g. mypurecloud.com, mypurecloud.de, etc.
};
ConnectionManager connectionManager = new ConnectionManager(credentials);

// 
UsersApi usersApi = new UsersApi(connectionManager);

var users = await usersApi.GetUsers();
Console.WriteLine("Users: " + users.Entities?.Count());

```
### With dependecy injection

You could also use the API in your service or Web app with DI:

Example:

```csharp

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// Read the credentials into configuration
builder.Configuration.AddJsonFile("credentials.json");
builder.Services.Configure<GenesysCloudCredentials>(builder.Configuration.GetSection("Credentials"));

// Register ConnectionManager
builder.Services.AddSingleton<ConnectionManager>();

// Register the APIs you want to use
builder.Services.AddTransient<UsersApi>();

// Register your service
builder.Services.AddHostedService<MyService>();

using IHost host = builder.Build();
```

To use the API add it to the constructor of your service:

```csharp
public MyService(UserApi userApi, ...)
```
