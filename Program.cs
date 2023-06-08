
string swaggerfile = "publicapi-v2-latest.json";
string targetNamespace = "GenesysCloud.Client.V2";
if(args.Length >= 3) {
    targetNamespace = args[2];
}
if(args.Length < 2) {
    Console.WriteLine("Syntax: ApiGenerator <hostname> <targetfolder>");
    return;
}
string hostname = args[0];
string targetFolder = args[1];

if(!Directory.Exists(targetFolder)) {
    Console.WriteLine($"Could not find folder {targetFolder}. Please create it first");
    return;
}

HttpClient client = new HttpClient();
client.BaseAddress = new Uri($"https://{hostname}");
var response = await client.GetAsync("/api/v2/docs/swagger");

if(response.IsSuccessStatusCode) {
    File.WriteAllText(swaggerfile, await response.Content.ReadAsStringAsync());
}
else {
    Console.WriteLine($"Error {response.StatusCode} loading swagger file: " + response.ReasonPhrase);
    return;
}

File.WriteAllText(Path.Combine(targetFolder, "ApiException.cs"), File.ReadAllText("ApiException.txt").Replace("{TargetNamespace}", targetNamespace));
File.WriteAllText(Path.Combine(targetFolder, "AuthTokenInfo.cs"), File.ReadAllText("AuthTokenInfo.txt").Replace("{TargetNamespace}", targetNamespace));
File.WriteAllText(Path.Combine(targetFolder, "ConnectionManager.cs"), File.ReadAllText("ConnectionManager.txt").Replace("{TargetNamespace}", targetNamespace));
File.WriteAllText(Path.Combine(targetFolder, "DateTimeInterval.cs"), File.ReadAllText("DateTimeInterval.txt").Replace("{TargetNamespace}", targetNamespace));
File.WriteAllText(Path.Combine(targetFolder, "GenesysCloudCredentials.cs"), File.ReadAllText("GenesysCloudCredentials.txt").Replace("{TargetNamespace}", targetNamespace));
File.WriteAllText(Path.Combine(targetFolder, "JsonEnumMemberStringEnumConverter.cs"), File.ReadAllText("JsonEnumMemberStringEnumConverter.txt").Replace("{TargetNamespace}", targetNamespace));
if (!File.Exists(Path.Combine(targetFolder, $"{targetNamespace}.csproj"))) {
    File.WriteAllText(Path.Combine(targetFolder, $"{targetNamespace}.csproj"), File.ReadAllText("Project.txt"));
}
ApiGenerator.ApiGenerator apiGenerator = new ApiGenerator.ApiGenerator(targetNamespace, targetFolder);

apiGenerator.ParseSwagger(swaggerfile);

apiGenerator.CreateApis();

apiGenerator.CreateModels();

apiGenerator.WriteDefinitionsJson();

apiGenerator.WriteDataDefinitions();

apiGenerator.WritePathsDefinitions();

//return;
