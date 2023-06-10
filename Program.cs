
using CommandLine;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using ApiGenerator;

string swaggerfile = "publicapi-v2-latest.json";
string targetNamespace = "GenesysCloud.Client.V2";

CommandLine.Parser.Default.ParseArguments<SwaggerOptions, ReadNotificationOptions, NotificationOptions>(args)
    .MapResult(
      (SwaggerOptions opts) => RunSwagger(opts),
      (ReadNotificationOptions opts) => RunReadNotificationInfo(opts),
      (NotificationOptions opts) => RunNotification(opts),
      errs => 1);



int RunSwagger(SwaggerOptions opts) {
    if(!Directory.Exists(opts.TargetFolder)) {
        Console.WriteLine($"Could not find folder {opts.TargetFolder}. Please create it first");
        return 1;
    }
    HttpClient client = new HttpClient();
    client.BaseAddress = new Uri($"https://{opts.Hostname}");
    var response = client.GetAsync("/api/v2/docs/swagger").Result;

    if (response.IsSuccessStatusCode) {
        File.WriteAllText(swaggerfile, response.Content.ReadAsStringAsync().Result);
    }
    else {
        Console.WriteLine($"Error {response.StatusCode} loading swagger file: " + response.ReasonPhrase);
        return 1;
    }
    File.WriteAllText(Path.Combine(opts.TargetFolder, "ApiException.cs"), File.ReadAllText("ApiException.txt").Replace("{TargetNamespace}", targetNamespace));
    File.WriteAllText(Path.Combine(opts.TargetFolder, "AuthTokenInfo.cs"), File.ReadAllText("AuthTokenInfo.txt").Replace("{TargetNamespace}", targetNamespace));
    File.WriteAllText(Path.Combine(opts.TargetFolder, "ConnectionManager.cs"), File.ReadAllText("ConnectionManager.txt").Replace("{TargetNamespace}", targetNamespace));
    File.WriteAllText(Path.Combine(opts.TargetFolder, "DateTimeInterval.cs"), File.ReadAllText("DateTimeInterval.txt").Replace("{TargetNamespace}", targetNamespace));
    File.WriteAllText(Path.Combine(opts.TargetFolder, "GenesysCloudCredentials.cs"), File.ReadAllText("GenesysCloudCredentials.txt").Replace("{TargetNamespace}", targetNamespace));
    File.WriteAllText(Path.Combine(opts.TargetFolder, "JsonEnumMemberStringEnumConverter.cs"), File.ReadAllText("JsonEnumMemberStringEnumConverter.txt").Replace("{TargetNamespace}", targetNamespace));
    if (!File.Exists(Path.Combine(opts.TargetFolder, $"{targetNamespace}.csproj"))) {
        File.WriteAllText(Path.Combine(opts.TargetFolder, $"{targetNamespace}.csproj"), File.ReadAllText("Project.txt"));
    }
    if(opts.TargetNamespace == null) {
        Console.WriteLine("Missing target namespace");
        return 1;
    }
    try {
        ApiGenerator.ApiGenerator apiGenerator = new ApiGenerator.ApiGenerator(opts.TargetNamespace, opts.TargetFolder);
        apiGenerator.ParseSwagger(swaggerfile);
        apiGenerator.CreateApis();
        apiGenerator.CreateModels();
        apiGenerator.WriteDefinitionsJson();
        apiGenerator.WriteDataDefinitions();
        apiGenerator.WritePathsDefinitions();
    }
    catch(Exception ex) {
        Console.WriteLine("Error creating API: ", ex.Message);
        return 1;
    }
    return 0;
}


int RunReadNotificationInfo(ReadNotificationOptions opts) {
    HttpClient _httpClient = new HttpClient {
        BaseAddress = new Uri($"https://api.{opts.Environment}")
    };
    //await GetToken("6c945a33-3c3a-4ad0-a51e-7c49768e19d0", "GvtSkAVB4HmAtKnC6nIz9Q-pIDGj30d_WjcquU9ZCbU", environment);
    var basicAuthauth = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{opts.ClientId}:{opts.ClientSecret}"));
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthauth);
    var form = new List<KeyValuePair<string, string>>();
    form.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

    var response = _httpClient.PostAsync($"https://login.{opts.Environment}/oauth/token", new FormUrlEncodedContent(form)).Result;
    response.EnsureSuccessStatusCode();
    var _tokenInfo = response.Content.ReadFromJsonAsync<AuthTokenInfo>().Result;
    if (_tokenInfo == null) {
        Console.WriteLine("Error getting token");
        return 1;
    }
    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokenInfo.AccessToken);

    //logger.LogInformation("Application started");

    //var api = new NotificationsApi(new ConnectionManager(new GenesysCloudCredentials() { ClientId = "6c945a33-3c3a-4ad0-a51e-7c49768e19d0", ClientSecret = "GvtSkAVB4HmAtKnC6nIz9Q-pIDGj30d_WjcquU9ZCbU", Environment = "mypurecloud.de" }), loggerFactory.CreateLogger<NotificationsApi>());

    //response = await GetNotificationsAvailabletopics();

    var requestPath = "/api/v2/notifications/availabletopics";
    var q_ = new QueryBuilder();
    q_.Add("expand", "description,visibility,transports,topicParameters,schema");

    var uri = new UriBuilder(_httpClient.BaseAddress);
    uri.Path = requestPath;
    uri.Query = q_.ToString();

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
    File.WriteAllText("notificationSchema.json", JsonSerializer.Serialize(result, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true }));
    return 0;
}
int RunNotification(NotificationOptions opts) {
    if (!Directory.Exists(opts.TargetFolder)) {
        Console.WriteLine($"Could not find folder {opts.TargetFolder}. Please create it first.");
        return 1;
    }
    if (!File.Exists(opts.NotificationFile)) {
        Console.WriteLine($"Could not find {opts.NotificationFile}.");
        return 1;
    }
    if (opts.TargetNamespace == null) {
        Console.WriteLine("Missing target namespace");
        return 1;
    }
    ApiGenerator.ApiGenerator apiGenerator = new ApiGenerator.ApiGenerator(opts.TargetNamespace, opts.TargetFolder);

    apiGenerator.ParseNotificationSwagger(opts.NotificationFile);
    apiGenerator.CreateNotificationDefinitions();
    apiGenerator.WriteDefinitionsJson();
    apiGenerator.WriteNotificationDefinitions();
    return 0;
}
[Verb("readnotification", HelpText = "Read notification definitions")]
class ReadNotificationOptions {
    [Option("clientId", Required = true, HelpText = "The client id for the request.")]
    public string? ClientId { get; set; }

    [Option("clientSecret", Required = true, HelpText = "The client secret for the request")]
    public string? ClientSecret { get; set; }

    [Option("environment", Required = true,  HelpText = "The evvironment to get the informantion from (Example: mypurecloud.de).")]
    public string? Environment { get; set; }
}

[Verb("notification", HelpText = "Proceed notification definitions")]
class NotificationOptions {
    [Option('i', "inputFile", Required = true, HelpText = "The notification definitions from NotificationReader.")]
    public string? NotificationFile { get; set; }

    // Omitting long name, defaults to name of property, ie "--verbose"
    [Option('t', "targetFolder", Required = true, HelpText = "The target folder to generate api files")]
    public string? TargetFolder { get; set; }

    [Option("namespace",
      Default = "GenesysCloud.Client.V2",
      HelpText = "The namespace of the api classes.")]
    public string? TargetNamespace { get; set; }
}
[Verb("swagger", HelpText = "Load and proceed swagger of api.")]
public class SwaggerOptions {
    [Option('h', "hostname", Required = true, HelpText = "The hostname to read the swagger (Example: api.mypurecloud.de).")]
    public string? Hostname { get; set; }

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

public class AvailableTopicEntityListing {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("entities")]
    public IEnumerable<AvailableTopic>? Entities { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
public class AvailableTopic {
    [JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]
    public enum VisibilityConstant {
        [JsonEnumName("Public")]
        Public,
        [JsonEnumName("Preview")]
        Preview,
    }
    [JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]
    public enum TransportsConstant {
        [JsonEnumName("All")]
        All,
        [JsonEnumName("Websocket")]
        Websocket,
        [JsonEnumName("EventBridge")]
        EventBridge,
        [JsonEnumName("ProcessAutomation")]
        ProcessAutomation,
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    //[JsonPropertyName("permissionDetails")]
    //public IEnumerable<PermissionDetails>? PermissionDetails { get; set; }
    [JsonPropertyName("requiresPermissions")]
    public IEnumerable<string>? RequiresPermissions { get; set; }
    [JsonPropertyName("requiresDivisionPermissions")]
    public bool? RequiresDivisionPermissions { get; set; }
    [JsonPropertyName("requiresAnyValidator")]
    public bool? RequiresAnyValidator { get; set; }
    [JsonPropertyName("enforced")]
    public bool? Enforced { get; set; }
    [JsonPropertyName("visibility")]
    public VisibilityConstant? Visibility { get; set; }
    [JsonPropertyName("schema")]
    public object? Schema { get; set; }
    [JsonPropertyName("requiresCurrentUser")]
    public bool? RequiresCurrentUser { get; set; }
    [JsonPropertyName("requiresCurrentUserOrPermission")]
    public bool? RequiresCurrentUserOrPermission { get; set; }
    [JsonPropertyName("transports")]
    public IEnumerable<TransportsConstant>? Transports { get; set; }
    [JsonPropertyName("publicApiTemplateUriPaths")]
    public IEnumerable<string>? PublicApiTemplateUriPaths { get; set; }
    [JsonPropertyName("topicParameters")]
    public IEnumerable<string>? TopicParameters { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
