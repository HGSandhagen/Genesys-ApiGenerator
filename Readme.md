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

The generator will create some files in the current directory:
- publicapi-v2-latest.json: The swagger file downloaded from Genesys.
- apis.json: The definition of the api calls parsed from swagger file.
- models.json: The definition of the data models parsed from the swagger file.
- notificationSchema.json: The definitons of the topic data read from Genesys (step 2, see avove)
- notifications.json: The notification data parsed from the topic data.

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
