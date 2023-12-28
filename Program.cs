
using CommandLine;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using System.Reflection;
using System.Text;
using System.Globalization;
using ApiGenerator;

string build = $"{DateTime.UtcNow.Year}.{ISOWeek.GetWeekOfYear(DateTime.Now)}";
string swaggerfile = $"publicapi-v2-{build}.json";
string notificationFile = $"notificationSchema-{build}.json";
//var appDirectory = AppContext.BaseDirectory;
var workingDirectory = Directory.GetCurrentDirectory();
var assembly = Assembly.GetEntryAssembly();
if(assembly == null) {
    Console.WriteLine("Error: Could not get assembly");
    return;
}
CommandLine.Parser.Default.ParseArguments<ReadSwaggerOptions, ReadNotificationOptions, GenerateOptions, AllOptions>(args)
    .MapResult(
      (ReadSwaggerOptions opts) => RunReadSwagger(opts),
      (ReadNotificationOptions opts) => RunReadNotification(opts),
      (GenerateOptions opts) => RunGenerate(opts),
      (AllOptions opts) => RunAll(opts),
      errs => 1);



int RunReadSwagger(ReadSwaggerOptions opts) {
    //if(!Directory.Exists(opts.TargetFolder)) {
    //    Console.WriteLine($"Could not find folder {opts.TargetFolder}. Please create it first");
    //    return 1;
    //}
    HttpClient client = new() {
        BaseAddress = new Uri($"https://{opts.Hostname}")
    };
    var response = client.GetAsync("/api/v2/docs/swagger").Result;

    if (response.IsSuccessStatusCode) {
        File.WriteAllText(swaggerfile, response.Content.ReadAsStringAsync().Result);
    }
    else {
        Console.WriteLine($"Error {response.StatusCode} loading swagger file: " + response.ReasonPhrase);
        return 1;
    }
    // if(opts.TargetNamespace == null) {
    //    Console.WriteLine("Missing target namespace");
    //    return 1;
    //}



    try {
        ApiGenerator.ApiGenerator apiGenerator = new();
        apiGenerator.ParseSwagger(swaggerfile);
        apiGenerator.CreateApis();
        apiGenerator.CreateModels();
        apiGenerator.WriteDefinitionsJson(build);
    }
    catch(Exception ex) {
        Console.WriteLine("Error creating API: " + ex.Message);
        return 1;
    }
    return 0;
}




int RunReadNotification(ReadNotificationOptions opts) {
    HttpClient _httpClient = new() {
        BaseAddress = new Uri($"https://api.{opts.Environment}")
    };
    //await GetToken("6c945a33-3c3a-4ad0-a51e-7c49768e19d0", "GvtSkAVB4HmAtKnC6nIz9Q-pIDGj30d_WjcquU9ZCbU", environment);
    var basicAuthauth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{opts.ClientId}:{opts.ClientSecret}"));
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthauth);
    var form = new List<KeyValuePair<string, string>> {
        new("grant_type", "client_credentials")
    };

    var response = _httpClient.PostAsync($"https://login.{opts.Environment}/oauth/token", new FormUrlEncodedContent(form)).Result;
    response.EnsureSuccessStatusCode();
    var _tokenInfo = response.Content.ReadFromJsonAsync<AuthTokenInfo>().Result;
    if (_tokenInfo == null) {
        Console.WriteLine("Error getting token");
        return 1;
    }
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenInfo.AccessToken);

    var requestPath = "/api/v2/notifications/availabletopics";
    var q_ = new QueryBuilder {
        { "expand", "description,visibility,transports,topicParameters,schema" }
    };

    var uri = new UriBuilder(_httpClient.BaseAddress) {
        Path = requestPath,
        Query = q_.ToString()
    };

    HttpRequestMessage request = new(HttpMethod.Get, uri.Uri);
    response = _httpClient.SendAsync(request).Result;
    if (((int)response.StatusCode) != 200) {
        Console.WriteLine($"Error {response.StatusCode} on reading available topics: {response.Content.ReadAsStringAsync().Result}");
    }
    var result = response.Content.ReadFromJsonAsync<AvailableTopicEntityListing>().Result;
    if (result == null) {
        Console.WriteLine("GetNotificationsAvailabletopics returned empty body");
        return 1;
    }
#pragma warning disable CA1869 // Cache and reuse 'JsonSerializerOptions' instances
    JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true };
#pragma warning restore CA1869 // Cache and reuse 'JsonSerializerOptions' instances
    File.WriteAllText(notificationFile, JsonSerializer.Serialize(result, options));
    ApiGenerator.ApiGenerator apiGenerator = new();
    apiGenerator.ParseNotificationSwagger(notificationFile);
    apiGenerator.CreateNotificationDefinitions();
    apiGenerator.WriteDefinitionsJson(build);

    return 0;
}
int RunGenerate(GenerateOptions opts) {
    if (!Directory.Exists(opts.TargetFolder)) {
        Console.WriteLine($"Could not find folder {opts.TargetFolder}. Please create it first.");
        return 1;
    }
    var notificationSchema = Directory.GetFiles(".","notificationSchema-*.json").OrderByDescending(p => p).FirstOrDefault();
    bool withNotification = File.Exists(notificationSchema);
    if (!withNotification) {
        Console.WriteLine($"Could not find {notificationSchema}.");
    }
    if (opts.TargetNamespace == null) {
        Console.WriteLine("Missing target namespace");
        return 1;
    }
    ApiGenerator.ApiGenerator apiGenerator = new();

    apiGenerator.Generate(opts.TargetFolder, opts.TargetNamespace);

    CreateCodeFile("ApiGenerator.ApiException.txt", Path.Combine(opts.TargetFolder, "ApiException.cs"), opts.TargetNamespace);
    CreateCodeFile("ApiGenerator.AuthTokenInfo.txt", Path.Combine(opts.TargetFolder, "AuthTokenInfo.cs"), opts.TargetNamespace);
    CreateCodeFile("ApiGenerator.ConnectionManager.txt", Path.Combine(opts.TargetFolder, "ConnectionManager.cs"), opts.TargetNamespace);
    CreateCodeFile("ApiGenerator.DateTimeInterval.txt", Path.Combine(opts.TargetFolder, "DateTimeInterval.cs"), opts.TargetNamespace);
    CreateCodeFile("ApiGenerator.GenesysCloudCredentials.txt", Path.Combine(opts.TargetFolder, "GenesysCloudCredentials.cs"), opts.TargetNamespace);
    CreateCodeFile("ApiGenerator.JsonEnumMemberStringEnumConverter.txt", Path.Combine(opts.TargetFolder, "JsonEnumMemberStringEnumConverter.cs"), opts.TargetNamespace);
    if (!File.Exists(Path.Combine(opts.TargetFolder, $"{opts.TargetNamespace}.csproj"))) {
        CreateCodeFile("ApiGenerator.Project.txt", Path.Combine(opts.TargetFolder, $"{opts.TargetNamespace}.csproj"), opts.TargetNamespace);

        //File.WriteAllText(Path.Combine(opts.TargetFolder, $"{opts.TargetNamespace}.csproj"), File.ReadAllText(Path.Combine(appDirectory, "Project.txt")));
    }
    if (withNotification) {

        CreateCodeFile("ApiGenerator.NotificationChannel.txt", Path.Combine(opts.TargetFolder, "NotificationChannel.cs"), opts.TargetNamespace);
        CreateCodeFile("ApiGenerator.NotificationData.txt", Path.Combine(opts.TargetFolder, "NotificationData.cs"), opts.TargetNamespace);
        CreateCodeFile("ApiGenerator.NotificationEvent.txt", Path.Combine(opts.TargetFolder, "NotificationEvent.cs"), opts.TargetNamespace);
        CreateCodeFile("ApiGenerator.NotificationMetadata.txt", Path.Combine(opts.TargetFolder, "NotificationMetadata.cs"), opts.TargetNamespace);
        CreateCodeFile("ApiGenerator.Notifications.txt", Path.Combine(opts.TargetFolder, "Notifications.cs"), opts.TargetNamespace);
        CreateCodeFile("ApiGenerator.TopicTypeInfo.txt", Path.Combine(opts.TargetFolder, "TopicTypeInfo.cs"), opts.TargetNamespace);
    }

    return 0;
}
void CreateCodeFile(string resourceName, string fileName, string targetNamespace) {
    if (assembly == null) {
        return;
    }
    var resourceStream = assembly.GetManifestResourceStream(resourceName);
    if (resourceStream == null) {
        Console.WriteLine($"Could not find resource {resourceName}");
        return;
    }
    using (var reader = new StreamReader(resourceStream, Encoding.UTF8)) {
        File.WriteAllText(fileName, reader.ReadToEnd().Replace("{TargetNamespace}", targetNamespace));
    }
}


int RunAll(AllOptions opts) {
    ReadSwaggerOptions swaggerOptions = new() {
        Hostname = opts.Hostname
    };
    int i = RunReadSwagger(swaggerOptions);
    if(i > 0) {
        return i;
    }
    if (!string.IsNullOrEmpty(opts.Environment) && !string.IsNullOrEmpty(opts.ClientId) && !string.IsNullOrEmpty(opts.ClientSecret)) {
        ReadNotificationOptions notificationOptions = new() {
            ClientId = opts.ClientId,
            ClientSecret = opts.ClientSecret,
            Environment = opts.Environment
        };

        i = RunReadNotification(notificationOptions);
        if (i > 0) {
            return i;
        }
    }
    GenerateOptions generateOptions = new() {
        TargetFolder = opts.TargetFolder,
        TargetNamespace = opts.TargetNamespace
    };
    return RunGenerate(generateOptions);
}
namespace ApiGenerator {
    [Verb("generate", HelpText = "Generate API files")]
    class GenerateOptions {
        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option('t', "targetFolder", Required = true, HelpText = "The target folder to generate api files")]
        public string? TargetFolder { get; set; }

        [Option("namespace",
          Default = "GenesysCloud.Client.V2",
          HelpText = "The namespace of the api classes.")]
        public string? TargetNamespace { get; set; }
    }
    [Verb("readswagger", HelpText = "Load and proceed swagger of api.")]
    public class ReadSwaggerOptions {
        [Option('h', "hostname", Required = true, HelpText = "The hostname to read the swagger (Example: api.mypurecloud.de).")]
        public string? Hostname { get; set; }
    }

    [Verb("readnotification", HelpText = "Load and proceed notification data of api.")]
    public class ReadNotificationOptions {

        [Option("clientId", Required = false, HelpText = "The client id for the request.")]
        public string? ClientId { get; set; }

        [Option("clientSecret", Required = false, HelpText = "The client secret for the request")]
        public string? ClientSecret { get; set; }

        [Option("environment", Required = false, HelpText = "The evironment to get the informantion from (Example: mypurecloud.de).")]
        public string? Environment { get; set; }

        //// Omitting long name, defaults to name of property, ie "--verbose"
        //[Option('t', "targetFolder", Required = true, HelpText = "The target folder to generate api files")]
        //public string? TargetFolder { get; set; }

        //[Option("namespace",
        //  Default = "GenesysCloud.Client.V2",
        //  HelpText = "The namespace of the api classes.")]
        //public string? TargetNamespace { get; set; }

        //[Value(0, MetaName = "offset", HelpText = "File offset.")]
        //public long? Offset { get; set; }

    }
    [Verb("all", HelpText = "Load and proceed swagger and notifications of api. Generate files.")]
    public class AllOptions {
        [Option('h', "hostname", Required = true, HelpText = "The hostname to read the swagger (Example: api.mypurecloud.de).")]
        public string? Hostname { get; set; }

        [Option("clientId", Required = true, HelpText = "The client id for the request.")]
        public string? ClientId { get; set; }

        [Option("clientSecret", Required = true, HelpText = "The client secret for the request")]
        public string? ClientSecret { get; set; }

        [Option("environment", Required = true, HelpText = "The evironment to get the informantion from (Example: mypurecloud.de).")]
        public string? Environment { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option('t', "targetFolder", Required = true, HelpText = "The target folder to generate api files")]
        public string? TargetFolder { get; set; }

        [Option("namespace",
          Default = "GenesysCloud.Client.V2",
          HelpText = "The namespace of the api classes.")]
        public string? TargetNamespace { get; set; }

        //[Value(0, MetaName = "offset", HelpText = "File offset.")]
        //public long? Offset { get; set; }

    }

    class AuthTokenInfo {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}