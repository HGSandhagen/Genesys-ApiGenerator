using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiGenerator {
    internal class ApiGenerator {
        private string _namespace;
        private string _targetFolder;
        private ApiDefaults _apiDefaults = new();
        private List<SwaggerApi> _swaggerApis = new();
        List<SwaggerDefinition> _swaggerDefinitions = new List<SwaggerDefinition>();
        private string? _swagger;
        private string? _host;
        private ApiInfo? _info;
        private List<DefinitionModel> _models = new();
        private Dictionary<string, List<ApiOperation>> _apis = new();
        public ApiGenerator(string targetNamespace, string targetFolder) {
            _namespace = targetNamespace;
            if (!Directory.Exists(targetFolder)) {
                Directory.CreateDirectory(targetFolder);
            }
            _targetFolder = targetFolder;
        }
        public ApiDefaults ApiDefaults { get => _apiDefaults; }
        public void WriteDefinitionsJson() {
            //File.WriteAllText("models.json", JsonSerializer.Serialize(_models, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true }));
            File.WriteAllText("apis.json", JsonSerializer.Serialize(_apis, new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true }));

        }

        public void ParseSwagger(string swaggerfile) {
            if (!File.Exists(swaggerfile)) {
                throw new ArgumentException("Could not find file \"" + swaggerfile + "\"");
            }
            JsonNode? document;
            try {
                document = JsonNode.Parse(File.ReadAllText(swaggerfile));
            }
            catch (Exception ex) {
                throw new Exception($"Error parsing file \"{swaggerfile}\"", ex);
            }
            if (document == null) {
                throw new Exception($"Error parsing file \"{swaggerfile}\"");
            }
            JsonNode root = document.Root;
            if (root is JsonObject) {
                foreach (var o in root.AsObject()) {
                    switch (o.Key) {
                        case "paths":
                            if (o.Value is JsonObject) {
                                foreach (var p in o.Value.AsObject()) {
                                    if (p.Value != null && p.Value is JsonObject) {
                                        if (p.Key.StartsWith("/")) {
                                            var api = new SwaggerApi(p.Key, p.Value.AsObject());
                                            _swaggerApis.Add(api);
                                        }
                                        else {
                                            throw new Exception("Unknown path " + p.Key);
                                        }
                                    }
                                    else {
                                        Console.WriteLine("Path must not be an array");
                                    }
                                }
                            }
                            else {
                                throw new Exception("Path as Array???");
                            }
                            break;
                        case "definitions":
                            if (o.Value is JsonObject) {
                                foreach (var p in o.Value.AsObject()) {
                                    //Console.WriteLine("Definition: " + p.Key);
                                    if (p.Value != null && p.Value is JsonObject) {
                                        _swaggerDefinitions.Add(new SwaggerDefinition(p.Key, (JsonObject)p.Value));
                                    }
                                    else {
                                        Console.WriteLine("Definition value must be an object");
                                    }
                                }
                            }
                            else {
                                throw new Exception("definition as Array???");
                            }
                            break;
                        case "swagger":
                            // TODO: Check swagger version
                            _swagger = o.Value?.ToString();
                            break;
                        case "info":
                            if (o.Value is JsonObject) {
                                _info = new ApiInfo((JsonObject)o.Value);
                            }
                            break;
                        case "host":
                            _host = o.Value?.ToString();
                            break;
                        case "tags":
                            if (o.Value is JsonArray) {
                                List<TagDescription> l = new();
                                foreach (var item in o.Value.AsArray()) {
                                    if (item is JsonObject) {
                                        l.Add(ParseTag(item.AsObject()));
                                    }
                                }
                                _apiDefaults.Tags = l.ToArray();
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }
                            break;
                        case "schemes":
                            if (o.Value is JsonArray) {
                                List<string> l = new();
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Schemes = l.ToArray();
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }

                            break;
                        case "consumes":
                            if (o.Value is JsonArray) {
                                List<string> l = new();
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Consumes = l.ToArray();
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }

                            break;
                        case "produces":
                            if (o.Value is JsonArray) {
                                List<string> l = new();
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Produces = l.ToArray();
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }

                            break;
                        case "securityDefinitions":
                            if (o.Value is JsonObject) {
                                List<SecurityScheme> l = new();
                                foreach (var item in o.Value.AsObject()) {
                                    if (item.Value is JsonObject) {
                                        l.Add(ParseSecurityObject(item.Key, (JsonObject)item.Value));
                                    }
                                }
                                _apiDefaults.SecuritySchemes = l.ToArray();
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }
                            break;
                        case "responses":
                            // TODO: default responses
                            break;
                        case "externalDocs":
                            if (o.Value is JsonObject) {
                                _apiDefaults.ExternalDocs = ParseExternalDocs((JsonObject)o.Value);
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }
                            break;
                        default:
                            Console.WriteLine("TODO: " + o.Key);
                            break;
                    }
                }
            }
        }

        public void CreateApis() {

            foreach (var item in _swaggerApis) {
                string apiName = item.Path.Substring("/api/v2/".Length);
                if (apiName.Contains('/')) {
                    apiName = apiName.Substring(0, apiName.IndexOf("/"));
                }
                if (_apis.ContainsKey(apiName)) {
                    _apis[apiName].AddRange(item.Operations.Select(p => CreateApiOperation(p)).ToList());
                }
                else {
                    _apis.Add(apiName, item.Operations.Select(p => CreateApiOperation(p)).ToList());
                }
            }

        }
        public void CreateModels() {
            foreach (var item in _swaggerDefinitions) {
                DefinitionModel def = new DefinitionModel(item);
                _models.Add(def);
            }
        }

        public void WriteDataDefinitions() {
            int i = 0;
            string modelFolder = Path.Combine(_targetFolder, "Model");
            if (!Directory.Exists(modelFolder)) {
                Directory.CreateDirectory(modelFolder);
            }
            else {
                //ClearDirectory(modelFolder);
            }
            foreach (var def in _models) {
                if (string.IsNullOrEmpty(def.Name)) {
                    throw new Exception("Name of data object must not be null");
                }
                Console.WriteLine(++i + ": " + def.Name);
                using (var writer = new StreamWriter(Path.Combine(modelFolder, def.Name + ".cs"))) {
                    writer.WriteLine("using System;");
                    writer.WriteLine("using System.Collections.Generic;");
                    writer.WriteLine("using System.Runtime.Serialization;");
                    writer.WriteLine("using System.Text.Json.Serialization;");
                    writer.WriteLine();
                    writer.WriteLine($"namespace {_namespace} {{");
                    WriteModelDefinition(def, writer, 1);
                    writer.WriteLine("}");
                }

            }
        }
        private ApiOperation CreateApiOperation(SwaggerOperation op) {
            var operation = new ApiOperation();
            operation.Id = op.OperationId;
            operation.IsDeprecated = op.Deprecated;
            if (op.Responses != null) {
                operation.Responses = op.Responses.Select(p => CreateApiResponse(operation.Id, p));
            }
            operation.Method = op.Method;
            operation.PurecloudMethodName = op.PurecloudMethodName;
            operation.Produces = op.Produces;
            operation.Consumes = op.Consumes;
            if (op.Parameters != null) {
                operation.Parameters = op.Parameters.Select(p => CreateApiParameter(p)).ToList();
            }
            operation.Path = op.Path;
            operation.Summary = (op.Summary ?? op.Description) ?? "";
            operation.Tags = op.Tags;
            if (op.Security != null) {
                operation.Permissions = op.Security.Select(p => new ApiPermisson() { PermissionType = p.Key, Permissions = p.Value });
            }
            return operation;
        }
        private ApiOperationParameter CreateApiParameter(SwaggerParameter p) {
            ApiOperationParameter param = new ApiOperationParameter();
            param.Name = p.Name;
            if (p.Ref != null) {
                param.TypeName = p.Ref.Replace("#/definitions/", "");
            }
            else if (p.Type != null) {
                TypeInfo t = p.GetTypeInfo(param.PName);
                param.TypeName = t.TypeName;
                param.IsCollection = t.IsCollection;
                if (t.EnumModel != null) {
                    // Workaround for wildcard enums
                    if (!t.EnumModel.EnumValues.Any(p => p.Contains("*"))) {
                        param.EnumValues = t.EnumModel.EnumValues.ToArray();
                    }
                    else {
                        param.TypeName = "string";// Console.WriteLine();
                    }
                }
            }
            else {
                throw new Exception("Missing type in operation parameter");
            }
            param.Description = p.Description;
            switch (p.In) {
                case "path":
                    param.Position = ApiOperationParameter.ParameterKind.Path; ;
                    break;
                case "query":
                    param.Position = ApiOperationParameter.ParameterKind.Query;
                    break;
                case "header":
                    param.Position = ApiOperationParameter.ParameterKind.Header;
                    break;
                case "body":
                    param.Position = ApiOperationParameter.ParameterKind.Body;
                    break;
                default:
                    throw new Exception($"Invalid parameter kind {p.In}");
            }
            return param;
        }
        private ApiOperationResponse CreateApiResponse(string operationName, SwaggerResponse r) {
            ApiOperationResponse response = new ApiOperationResponse();
            if (r.Content != null) {
                TypeInfo t = r.Content.GetTypeInfo(operationName);
                response.TypeName = t.TypeName;
                response.IsCollection = t.IsCollection;
                response.IsDictionary = t.IsDictionary;
                response.EnumModel = t.EnumModel;
            }
            response.Description = r.Description;
            response.ResponseCode = r.Name;
            response.ErrorCodes = r.IninErrorCodes;
            return response;
        }

        public void WritePathsDefinitions() {
            string apiFolder = Path.Combine(_targetFolder, "Api");
            if (!Directory.Exists(apiFolder)) {
                Directory.CreateDirectory(apiFolder);
            }
            int i = 0;
            StreamWriter? writer = null;
            //var groups = _apis.Select(p => p.Name).Distinct().OrderBy(p => p);
            foreach (var group in _apis) {
                Console.WriteLine(++i + ": " + group.Key);

                //if (!path.Name.StartsWith(group)) {
                string groupName = CreateName(group.Key);
                writer = new StreamWriter(Path.Combine(apiFolder, groupName + "Api.cs"));
                writer.WriteLine("using Microsoft.AspNetCore.Http.Extensions;");
                writer.WriteLine("using Microsoft.Extensions.Logging;");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using System.Net.Http;");
                writer.WriteLine("using System.Net.Http.Json;");
                writer.WriteLine("using System.Text.Json.Serialization;");
                writer.WriteLine("using System.Threading.Tasks;");
                writer.WriteLine();
                writer.WriteLine($"namespace {_namespace} {{");
                writer.WriteLine($"\tpublic class {groupName}Api {{");
                writer.WriteLine("\t\tprivate readonly ConnectionManager _connectionManager;");
                writer.WriteLine("\t\tprivate readonly ILogger _logger;");
                writer.WriteLine("\t\t/// <summary>");
                writer.WriteLine($"\t\t/// Initializes a new instance of the <see cref=\"{groupName}Api\"/> class.");
                writer.WriteLine("\t\t/// </summary>");
                writer.WriteLine("\t\t/// <returns></returns>");
                writer.WriteLine($"\t\tpublic {groupName}Api(ConnectionManager connectionManager, ILogger<{groupName}Api> logger) {{");
                writer.WriteLine("\t\t\t_connectionManager= connectionManager;");
                writer.WriteLine("\t\t\t_logger = logger;");
                writer.WriteLine("\t\t}");

                //var x = group.Value.Where(p => p.Parameters != null).SelectMany(p => p.Parameters).Where(p => p.EnumValues != null);



                foreach (var op in group.Value) {

                    //writer.WriteLine(path.Print(2));
                    WriteOperation(op, writer, 2);
                }
                writer.WriteLine("\t}");
                writer.WriteLine("}");
                writer.Close();
            }
        }
        private void WriteOperation(ApiOperation operation, StreamWriter writer, int indent) {
            string operationName = $"{operation.Id.Substring(0, 1).ToUpper()}{operation.Id.Substring(1)}";
            if (operation.Parameters?.Any() == true) {
                foreach (var item in operation.Parameters.Where(p => p.EnumValues != null)) {
                    WriteEnumDefinition(operationName + item.TypeName, item.EnumValues.ToArray(), writer, 2);
                    //writer.WriteIndent(indent + 1).WriteLine($"public enum {operationName + item.TypeName} {{ {string.Join(",", item.EnumValues)} }}");
                }
            }
            var enumResponse = operation.Responses?.Where(p => p.EnumModel != null).FirstOrDefault();
            if (enumResponse != null && enumResponse.EnumModel != null) {
                WriteEnumDefinition(enumResponse.TypeName, enumResponse.EnumModel.EnumValues.ToArray(), writer, 2);
            }
            if (operation.Summary != null) {
                writer.WriteIndent(indent + 1).WriteLine("/// <summary>");
                foreach (var item in operation.Summary.Split("\n")) {
                    writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                }
                writer.WriteIndent(indent + 1).WriteLine("/// </summary>");
                if (operation.Description != null) {
                    writer.WriteIndent(indent + 1).WriteLine("/// <remarks>");
                    foreach (var item in operation.Description.Split("\n")) {
                        writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                    }
                    if (operation.Permissions?.Any() == true) {
                        foreach (var item in operation.Permissions) {
                            if (item.Permissions != null) {
                                writer.WriteIndent(indent + 1).WriteLine($"/// Required permission: {item.PermissionType} of");
                                foreach (var p in item.Permissions) {
                                    writer.WriteIndent(indent + 1).WriteLine($"///    {p}");
                                }
                            }
                        }
                    }
                    writer.WriteIndent(indent + 1).WriteLine("/// </remarks>");
                }
            }
            else if (operation.Description != null) {
                writer.WriteIndent(indent + 1).WriteLine("/// <summary>");
                foreach (var item in operation.Description.Split("\n")) {
                    writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                }
                writer.WriteIndent(indent + 1).WriteLine("/// </summary>");
                if (operation.Permissions?.Any() == true) {
                    writer.WriteIndent(indent + 1).WriteLine("/// <remarks>");
                    foreach (var item in operation.Permissions) {
                        if (item.Permissions != null) {
                            writer.WriteIndent(indent + 1).WriteLine($"/// Required permission: {item.PermissionType} of");
                            foreach (var p in item.Permissions) {
                                writer.WriteIndent(indent + 1).WriteLine($"///    {p}");
                            }
                        }
                    }
                    writer.WriteIndent(indent + 1).WriteLine("/// </remarks>");
                }
            }
            // TODO: check successful response
            string response = "void";
            if (operation.Responses != null) {
                var res = operation.Responses.FirstOrDefault(p => p.ResponseCode == "default");
                if (res == null) {
                    res = operation.Responses.OrderBy(p => p.ResponseCode).First();
                }
                if (res.TypeName != null) {
                    response = res.TypeName;
                }
                else {
                    //Console.WriteLine(res.Name + " without type");
                }

            }
            if (response == "void") {
                writer.WriteIndent(indent + 1).Write("public async Task ");

            }
            else {
                writer.WriteIndent(indent + 1).Write($"public async Task<{response}> ");
            }
            writer.Write($"{operationName} (");
            if (operation.Parameters?.Any() == true) {
                writer.Write(string.Join(", ", operation.Parameters.OrderByDescending(p => p.IsRequired).Select(p => {
                    var typeName = p.TypeName;
                    if (p.EnumValues != null) {
                        typeName = operationName + typeName;
                    }
                    if (p.IsCollection) {
                        typeName = $"IEnumerable<{typeName}>";
                    }
                    if (p.IsRequired) {
                        return $"{typeName} {p.PName}";
                    }
                    else {
                        return $"{typeName}? {p.PName} = null";
                    }
                })));
            }

            writer.WriteLine(") {");
            writer.WriteIndent(indent + 2).WriteLine($"// {operation.Path}");
            writer.WriteIndent(indent + 2).WriteLine("var httpClient = await _connectionManager.GetClient();");
            writer.WriteIndent(indent + 2).WriteLine("if (httpClient.BaseAddress == null) {");
            writer.WriteIndent(indent + 3).WriteLine("throw new Exception(\"BasePath of HttpClient not set\");");
            writer.WriteIndent(indent + 2).WriteLine("}");
            if (operation.Parameters?.Any(p => p.Position == ApiOperationParameter.ParameterKind.Path) == true) {
                writer.WriteIndent(indent + 2).WriteLine($"var requestPath = $\"{operation.Path}\";");
            }
            else {
                writer.WriteIndent(indent + 2).WriteLine($"var requestPath = \"{operation.Path}\";");
            }

            if (operation.Parameters != null) {
                writer.WriteIndent(indent + 2).WriteLine("// Query params");
                writer.WriteIndent(indent + 2).WriteLine("var q_ = new QueryBuilder();");
                var queryParams = operation.Parameters?.Where(p => p.Position == ApiOperationParameter.ParameterKind.Query);
                if (queryParams != null) {
                    foreach (var item in queryParams) {
                        if (item.TypeName == "int" || item.TypeName == "bool" || item.TypeName == "float" || item.TypeName == "double" || item.TypeName == "long" || item.TypeName == "DateTimeInterval") {
                            if (item.IsRequired) {
                                writer.WriteIndent(indent + 2).WriteLine($"q_.Add(\"{item.Name}\", {item.PName}.ToString());");
                            }
                            else {
                                writer.WriteIndent(indent + 2).WriteLine($"if({item.PName} != null) {{");
                                writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", (({item.TypeName}){item.PName}).ToString());");
                                writer.WriteIndent(indent + 2).WriteLine("}");
                            }
                        }
                        else if (item.EnumValues != null) {
                            if (item.IsRequired) {
                                if (item.IsCollection) {
                                    writer.WriteIndent(indent + 2).WriteLine($"if({item.PName} != null && {item.PName}.Count() > 0) {{");
                                    writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", string.Join(\",\",{item.PName}.Select(p => p.GetAttribute<JsonEnumNameAttribute>().JsonName)));");
                                    writer.WriteIndent(indent + 2).WriteLine("}");
                                }
                                else {
                                    writer.WriteIndent(indent + 2).WriteLine($"q_.Add(\"{item.Name}\", {item.PName}.GetAttribute<JsonEnumNameAttribute>().JsonName);");
                                }
                            }
                            else {
                                if (item.IsCollection) {
                                    writer.WriteIndent(indent + 2).WriteLine($"if({item.PName} != null && {item.PName}.Count() > 0) {{");
                                    writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", string.Join(\",\",{item.PName}.Select(p => p.GetAttribute<JsonEnumNameAttribute>().JsonName)));");
                                    writer.WriteIndent(indent + 2).WriteLine("}");
                                }
                                else {
                                    writer.WriteIndent(indent + 2).WriteLine($"if({item.PName} != null) {{");
                                    writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", {item.PName}.GetAttribute<JsonEnumNameAttribute>().JsonName);");
                                    writer.WriteIndent(indent + 2).WriteLine("}");
                                }
                            }
                        }
                        else if (item.TypeName == "DateTime" || item.TypeName == "DateTimeOffset" || item.TypeName == "DateOnly") {
                            if (item.IsRequired) {
                                //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).Append("if(").Append(item.PName).Append(".HasValue && !string.IsNullOrEmpty(").Append(item.PName).AppendLine(".ToString())) {");
                                writer.WriteIndent(indent + 2).WriteLine($"q_.Add(\"{item.Name}\", (({item.TypeName}){item.PName}).ToString(\"yyyy-MM-ddTHH:mm:ssZ\"));");
                                //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("}");

                            }
                            else {
                                writer.WriteIndent(indent + 2).WriteLine($"if({item.PName}.HasValue && !string.IsNullOrEmpty({item.PName}.ToString())) {{");
                                writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"({item.Name}\", (({item.TypeName}){item.PName}).ToString(\"yyyy-MM-ddTHH:mm:ssZ\"));");
                                writer.WriteIndent(indent + 2).WriteLine("}");
                            }
                        }
                        //else if (item.IsEnum) {
                        //    sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).Append("if(!string.IsNullOrEmpty(").Append(item.Name).AppendLine(")) {");
                        //    sb.AppendJoin('\t', Enumerable.Repeat("", indent + 3)).Append("q_.Add(\"").Append(item.Name).Append("\", ").Append(item.Name).AppendLine(".ToString());");
                        //    sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("}");
                        //}
                        else if (item.IsCollection) {
                            writer.WriteIndent(indent + 2).WriteLine($"if({item.PName} != null && {item.PName}.Count() > 0) {{");
                            writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", string.Join(\",\",{item.PName}));");
                            writer.WriteIndent(indent + 2).WriteLine("}");
                        }
                        else {
                            writer.WriteIndent(indent + 2).WriteLine($"if(!string.IsNullOrEmpty({item.PName})) {{");
                            writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", {item.PName});");
                            writer.WriteIndent(indent + 2).WriteLine("}");
                        }
                    }
                }
            }
            var otherParams = operation.Parameters?.Where(p => p.Position != ApiOperationParameter.ParameterKind.Path && p.Position != ApiOperationParameter.ParameterKind.Query);
            if (otherParams != null) {
                foreach (var item in otherParams) {
                    writer.WriteIndent(indent + 2).WriteLine($"// {item.Position}: {item.TypeName} {item.Name}");
                }
            }
            writer.WriteLine();
            writer.WriteIndent(indent + 2).WriteLine("// make the HTTP request");
            writer.WriteIndent(indent + 2).WriteLine("var uri = new UriBuilder(httpClient.BaseAddress);");
            writer.WriteIndent(indent + 2).WriteLine("uri.Path = requestPath;");
            if (operation.Parameters?.Any(p => p.Position == ApiOperationParameter.ParameterKind.Query) == true) {
                writer.WriteIndent(indent + 2).WriteLine("uri.Query = q_.ToString();");
            }

            writer.WriteIndent(indent + 2).WriteLine($"using HttpRequestMessage request = new(HttpMethod.{operation.Method}, uri.Uri);");
            writer.WriteIndent(indent + 2).WriteLine("using var response = await httpClient.SendAsync(request);");

            //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("if (response.IsSuccessStatusCode) {");
            //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("}");
            //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("else {");
            if (operation.Responses != null) {
                foreach (var item in operation.Responses.UnionBy(_apiDefaults.Responses, p => p.ResponseCode)) {
                    var def = _apiDefaults.Responses.SingleOrDefault(p => p.ResponseCode == item.ResponseCode);
                    var typeName = item.TypeName;
                    var description = item.Description;
                    if (string.IsNullOrEmpty(description) && def != null) {
                        typeName = def.Description;
                    }
                    if (string.IsNullOrEmpty(typeName) && def != null) {
                        typeName = def.TypeName;
                    }
                    if (int.TryParse(item.ResponseCode, out var responseType)) {
                        if (responseType >= 400) {
                            writer.WriteIndent(indent + 2).WriteLine($"if (((int)response.StatusCode) == {responseType}) {{");
                            if (string.IsNullOrEmpty(typeName)) {
                                writer.WriteIndent(indent + 3).WriteLine("throw new ApiException(\"{description}\");");
                            }
                            else {
                                writer.WriteIndent(indent + 3).WriteLine("if(response.Content != null) {");
                                writer.WriteIndent(indent + 4).WriteLine($"throw new ApiException(\"Error {description} calling {CreateName(operation.Id)}\", await response.Content.ReadFromJsonAsync<{item.TypeName}>());");
                                writer.WriteIndent(indent + 3).WriteLine("}");
                            }
                            writer.WriteIndent(indent + 2).WriteLine("}");
                        }
                        else {
                            writer.WriteIndent(indent + 2).WriteLine($"if (((int)response.StatusCode) == {responseType}) {{");
                            if (response != "void") {
                                writer.WriteIndent(indent + 3).WriteLine($"var result = await response.Content.ReadFromJsonAsync<{response}>();");
                                if (response != "int" && item.EnumModel == null) {
                                    writer.WriteIndent(indent + 3).WriteLine("if(result == null) {");
                                    writer.WriteIndent(indent + 4).WriteLine($"throw new ApiException(\"{CreateName(operation.Id)} returned empty body\");");
                                    writer.WriteIndent(indent + 3).WriteLine("}");
                                }
                                writer.WriteIndent(indent + 3).WriteLine("return result;");
                            }
                            else {
                                writer.WriteIndent(indent + 3).WriteLine("return;");
                            }
                            writer.WriteIndent(indent + 2).WriteLine("}");
                        }
                    }
                    else if (item.ResponseCode == "default") {
                        if (response != "void") {
                            writer.WriteIndent(indent + 2).WriteLine($"var result = await response.Content.ReadFromJsonAsync<{response}>();");
                            if (response != "int") {
                                writer.WriteIndent(indent + 2).WriteLine("if(result == null) {");
                                writer.WriteIndent(indent + 2).WriteLine($"throw new ApiException(\"{CreateName(operation.Id)} returned empty body\");");
                                writer.WriteIndent(indent + 2).WriteLine("}");
                            }
                            writer.WriteIndent(indent + 2).WriteLine("return result;");
                        }
                    }
                    else {
                        throw new Exception("Error in writing operation " + operation.Path);
                    }
                }
            }

            //foreach (var item in _apiDefaults.Responses.Where(p => p.ResponseCode)) { 

            //}
            writer.WriteIndent(indent + 2).WriteLine($"throw new ApiException($\"Error {{response.StatusCode}} calling {CreateName(operation.Id)}\");");
            //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("}");


            //sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).AppendLine("response.EnsureSuccessStatusCode();");
            //if (response != "void") {
            //    sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).Append("return await response.Content.ReadFromJsonAsync<").Append(response).AppendLine(">();");
            //}
            //if (operation.Responses != null) {
            //    foreach (var item in operation.Responses.Where(p => p.Type != response)) {
            //        sb.AppendJoin('\t', Enumerable.Repeat("", indent + 2)).Append("// ").Append(item.Name).Append(": ").AppendLine(item.Type);
            //    }
            //}
            //sb.AppendLine();

            writer.WriteIndent(indent + 1).WriteLine("}");

            //writer.Write(sb.ToString());
            //return sb.ToString();
        }


        private SecurityScheme ParseSecurityObject(string key, JsonObject value) {
            string? schemeType = null;
            string? description = null;
            string? parameterName = null;
            string? in_ = null;
            string? flow = null;
            string? authorizationUrl = null;
            string? tokenUrl = null;
            IEnumerable<Scope>? scopes = null;
            foreach (var item in value.AsObject()) {
                switch (item.Key) {
                    case "type":
                        schemeType = item.Value?.ToString();
                        break;
                    case "description":
                        description = item.Value?.ToString();
                        break;
                    case "name":
                        parameterName = item.Value?.ToString();
                        break;
                    case "in":
                        in_ = item.Value?.ToString();
                        break;
                    case "flow":
                        flow = item.Value?.ToString();
                        break;
                    case "authorizationUrl":
                        authorizationUrl = item.Value?.ToString();
                        break;
                    case "tokenUrl":
                        tokenUrl = item.Value?.ToString();
                        break;
                    case "scopes":
                        if (item.Value is JsonObject) {
                            scopes = ParseScopes(item.Value.AsObject());
                        }
                        else {
                            throw new Exception("Security scopes must be a JsonObject");
                        }
                        break;
                    default:
                        throw new Exception("Unknown paramter in security scheme");
                }
            }
            switch (schemeType) {
                case "basic":
                    return new BasicSecurityScheme() { Description = description };
                case "apiKey":
                    if (string.IsNullOrEmpty(parameterName)) {
                        throw new Exception("Missing name in SecurityScheme");
                    }
                    if (string.IsNullOrEmpty(in_)) {
                        throw new Exception("Missing \"in\" in SecurityScheme");
                    }
                    return new ApiKeySecurityScheme(parameterName, in_);
                case "oauth2":
                    if (string.IsNullOrEmpty(flow)) {
                        throw new Exception("Missing flow in SecurityScheme");
                    }
                    if (string.IsNullOrEmpty(authorizationUrl)) {
                        throw new Exception("Missing authorizationUrl in SecurityScheme");
                    }
                    if (string.IsNullOrEmpty(tokenUrl)) {
                        if (flow != "implicit") {
                            throw new Exception("Missing tokenUrl in SecurityScheme");
                        }
                    }
                    if (scopes == null) {
                        throw new Exception("Missing scopes in SecurityScheme");
                    }
                    return new OAuth2SecurityScheme(flow, authorizationUrl, tokenUrl, scopes);
                default:
                    throw new Exception("Unknown security scheme type " + schemeType);
            }
        }
        private IEnumerable<Scope> ParseScopes(JsonObject scopes) {
            List<Scope> scopesList = new List<Scope>();
            foreach (var item in scopes.AsObject()) {
                if (item.Value == null) {
                    throw new Exception("Description of scope must not be empty");
                }
                scopesList.Add(new Scope(item.Key, item.Value.ToString()));
            }
            return scopesList;
        }
        private TagDescription ParseTag(JsonObject tag) {
            string? name = null;
            string? description = null;
            Documentation? externalDocs = null;
            foreach (var item in tag.AsObject()) {
                switch (item.Key) {
                    case "name":
                        name = item.Value?.ToString();
                        break;
                    case "description":
                        description = item.Value?.ToString();
                        break;
                    case "externalDocs":
                        if (item.Value is JsonObject) {
                            externalDocs = ParseExternalDocs(item.Value.AsObject());
                        }
                        else {
                            throw new Exception("ExternalDos must be an JsonObject");
                        }
                        break;
                    default:
                        throw new Exception("Unknown element " + item.Key + " in tags.");
                }
            }
            if (string.IsNullOrEmpty(name)) {
                throw new Exception("Name of tag must not be empty");
            }
            return new TagDescription(name) {
                Description = description,
                ExternalDocumentation = externalDocs
            };
        }
        private Documentation ParseExternalDocs(JsonObject externalDocs) {
            string? url = null;
            string? description = null;
            foreach (var item in externalDocs.AsObject()) {
                switch (item.Key) {
                    case "url":
                        url = item.Value?.ToString();
                        break;
                    case "description":
                        description = item.Value?.ToString();
                        break;
                    default:
                        throw new Exception("Unknown element " + item.Key + " in externalDocs.");
                }
            }
            if (string.IsNullOrEmpty(url)) {
                throw new Exception("Url of tag must not be empty");
            }
            return new Documentation(url) {
                Description = description
            };
        }

        void WriteModelDefinition(DefinitionModel model, StreamWriter writer, int indent) {

            if (model.Description != null) {
                writer.WriteIndent(indent + 1).WriteLine("/// <summary>");
                foreach (var item in model.Description.Split("\n")) {
                    writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                }
                writer.WriteIndent(indent + 1).WriteLine("/// </summary>");
            }
            writer.WriteIndent(indent + 2).WriteLine($"public class {model.Name} {{");
            if (model.Properties != null) {
                //foreach (var item in EnumDefinitions) {
                foreach (var item in model.EnumDefinitions) {
                    if (item.EnumValues != null) {
                        WriteEnumDefinition(item.Name, item.EnumValues.ToArray(), writer, indent + 1);
                    }
                }
                writer.WriteLine("#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");
                foreach (var item in model.Properties) {
                    WriteProperty(item, writer, indent + 1);
                }
                writer.WriteLine("#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.\r\n");
            }
            writer.WriteIndent(indent + 1).WriteLine("}");
            //return sb.ToString();
        }
        void WriteEnumDefinition(string name, string[] values, StreamWriter writer, int indent) {
            //StringBuilder sb = new();
            writer.WriteIndent(indent + 1).WriteLine("[JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]");
            writer.WriteIndent(indent + 1).WriteLine($"public enum {name} {{");
            foreach (var item in values) {
                writer.WriteIndent(indent + 2).WriteLine($"[JsonEnumName(\"{item}\")]");
                writer.WriteIndent(indent + 2).WriteLine($"{CreateName(item)},");
            }
            writer.WriteIndent(indent + 1).WriteLine("}");

            //return sb.ToString();
        }

        void WriteProperty(DefinitionPropertyModel param, StreamWriter writer, int indent) {
            //StringBuilder sb = new();
            if (param.Description != null) {
                writer.WriteIndent(indent + 1).WriteLine("/// <summary>");
                foreach (var item in param.Description.Split("\n")) {
                    writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                }
                writer.WriteIndent(indent + 2).WriteLine($"/// Required = {param.IsRequired}");
                writer.WriteIndent(indent + 1).WriteLine("/// </summary>");
            }
            writer.WriteIndent(indent + 1).WriteLine($"[JsonPropertyName(\"{param.JsonName}\")]");
            var notRequired = param.IsRequired ? "" : "?";
            if (param.IsCollection) {
                writer.WriteIndent(indent + 1).WriteLine($"public IEnumerable<{param.TypeName}>{notRequired} {param.Name} {{ get; set; }}");
            }
            else {
                writer.WriteIndent(indent + 1).WriteLine($"public {param.TypeName}{notRequired} {param.Name} {{ get; set; }}");
            }
            //return sb.ToString();
        }

        internal static string CreateName(string jsonName, bool lower = false) {
            string name = Regex.Replace(jsonName, "(-[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(-[0-9])", (p) => "_" + p.Value[1..]);
            name = Regex.Replace(name, "(_[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(/[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(:[A-Za-z0-9])", (p) => "_" + p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(\\$[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(\\.[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
            if (Char.IsNumber(name[0])) {
                name = "_" + name;
            }
            if (name == "override") {
                name = "@override";
            }
            if (!lower) {
                name = string.Concat(name[0].ToString().ToUpper(), name.AsSpan(1)); // FirstCharToUpper();
            }
            if (name == "GetType") {
                name += "_";
            }
            return name;
        }

    }

    public static class WriterExtensions {
        public static StreamWriter WriteIndent(this StreamWriter writer, int value) {
            if (writer == null) {
                throw new ArgumentNullException("Writer must not be null");
            }
            if (value < 0) {
                throw new ArgumentOutOfRangeException("value");
            }
            writer.Write(string.Join("\t", Enumerable.Repeat("", value)));
            return writer;
        }
    }
}