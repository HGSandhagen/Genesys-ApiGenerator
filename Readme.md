# API generator for Genesys Cloud API

This is an experimental application to genereate a C# library to access Genesys Cloud API.

> **Important:** This code is NOT created or maintained by Genesys. 

The goal is to create a SDK with
- run on .NET6 an above
- use third party packages as less as possible
- use standard logging mechanism (Microsoft.Extensions.ILogger)
  
At the moment it only generates a .NET project and source files.

To build the application clone the repo and run from the main directory ```dotnet publish -c Release -r <rid>``` where ```<rid>``` is the runtime identifier.
For possible RIDs see [Using RID](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog#using-rids).

Alternatively you could generate a nuget package and install it as a dotnet tool. For this run ```dotnet pack ApiGenerator.csproj -c Release -p= PublishSingleFile=false -o <output directory>```.

Example: ```dotnet pack ApiGenerator.csproj -c Release -p= PublishSingleFile=false -o ./nuget```.

To install the tool for your project call ```dotnet new tool-manifest``` to create a manifest file and then install the tool with ```dotnet tool install --add-source <package location> ApiGenerator```  
where 'package location' is the path to the created nuget package or the URL, where the package is uploaded.

Example: ```dotnet tool install --add-source .\Genesys-ApiGenerator\nuget ApiGenerator```

The generation of the API project works in 3 steps. To check changes in the downloaded data, the names of all genereated files contain 
the year and week of the dates, when they are generated: yyyy-ww with yyyy: the year and ww: the ISOWeek

1. Run ```ApiGenerator readswagger -h <hostname>``` with:
- hostname: The host to download the swagger file (example: api.mypurecloud.de)

Example: ```ApiGenerator -h api.mypurecloud.de ```

This will download the swagger file from https://api.mypurecloud.de/api/v2/docs/swagger, stores it in publicapi-v2-{build}.json and generate the description files.

2. The type information of notification events are not part of the swagger file. To get this informaten run ```ApiGenerator readnotification --clientId <cliendId> --clientSecret  <clientSecret> --environment <environment>``` with:
- clientId: The client id for the request.
- clientSecret: The clientSecret for the request.
- environment: The environment to get the information from (Example: mypurecloud.de).

Example: ```ApiGenerator readnotification --clientId ************** --clientSecret  ******************** --environment mypurecloud.de ```

This will login with the given credentials get the list of available topics from the api and store them in ```notificationSchema-{yyyy-ww}.json``` and generates the description file ```notifications-{yyyy-ww}.json``` in the current directory.

After this, the following files are generated in the currect directory:
- publicapi-v2-{yyyy-ww}.json: The swagger file downloaded from Genesys.
- apis.json-{yyyy-ww}: The definition of the api calls parsed from swagger file.
- models-{yyyy-ww}.json: The definition of the data models parsed from the swagger file.
- notificationSchema-{yyyy-ww}.json: The definitons of the topic data read from Genesys (step 2, see avove)
- notifications-{yyyy-ww}.json: The notification data parsed from the topic data.

3. To generate the the SDK code run ```ApiGenerator generate -t <target folder> [ --namespace <ApiNamespace>]``` with:
- target folder: The folder, where the project is created. 
- namespace (optional): The namespace of the generated code. Default: GenesysCloud.Client.V2

Example: ```ApiGenerator generate -t D:\MyProjects\GenesysApi --namespace MyNamespace ```

This will read the description files (api-*.json, models-*.json and notifications-*.json) and gerenate the SDK files in the target folder.

Alternatively you could run all steps at once with ```ApiGenerator all -h <hostname> --clientId ************** --clientSecret  ******************** --environment mypurecloud.de -t <target folder> [ --namespace <ApiNamespace>]```

To generate the SDK run ```dotnet build -c Release``` from target folder.

## Usage

### Console app

To use the api in a simple console app:
1. Create an instance of ```GenesysCloudCredentials``` which holds the client id, client secret and the environment for the api access.
2. Create an instance of ```ConnectionManager```.
3. Create an instance of the API you want to use.
4. Use the api.
 
> **_Attention:_**  You should not have credential values in your code. This is for demonstration only. In your application read it from a secure place (e.g. KeyVault).

Example:
```csharp
GenesysCloudCredentials credentials = new GenesysCloudCredentials() {
    ClientId = "<Enter your client id here>",
    ClientSecret = "<Enter your client secret here>",
    Environment = "<The environment of your organisation>" // e.g. mypurecloud.com, mypurecloud.de, etc.
};
ConnectionManager connectionManager = new ConnectionManager(credentials);

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

### Notifications

To use notifications initialize an instance of ```Notifications``` with ```ConnectionManager``` and optional logger as described above.
```csharp
Notifications notifications = new(connectionManager);
```

Create a channel to receive notification events:

```csharp
var channel = await notifications.CreateChannel();
```

Register an event handler to receive notifications (Example user presence):

```csharp
channel.NotificationReceived += (e) => {
    switch (e) {
        case PresenceEventV2UserPresence pe:
            Console.WriteLine($"{pe.NotificationUserId}: {pe.PresenceDefinition?.SystemPresence}");
            break;
        default:
            Console.WriteLine("Unexpected event " + e.GetType().Name + " received");
            break;
    }
};
```

Subscribe a notification (Example user presence of user 00000000-0000-0000-0000-000000000000):

```csharp
await channel.SetTopics(new List<ChannelTopic>() { new ChannelTopic() { Id = "v2.users.00000000-0000-0000-0000-000000000000.presence" } });
```
This open a websocket connection to receive the notifications.

Use ```AddTopics``` to add additional topics to the subscription. Use ```SetTopics``` to replace the list of topics in the subscription with a new one.

Call ```channel.DeleteTopics()``` or ```notifications.RemoveChannel``` to stop the receive of notifications and stop the underlying websocket.

To find the available topics and the corresponding ```NotificationEvent``` classes have a look at ```NotificationChannelTopicMap.cs```.
