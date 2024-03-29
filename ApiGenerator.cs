﻿using CommandLine.Text;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiGenerator {
    internal partial class ApiGenerator {
        //private readonly string _namespace;
        //private readonly string _targetFolder;
        private readonly ApiDefaults _apiDefaults = new();
        private readonly Dictionary<string, List<SwaggerOperation>> _swaggerapis = [];
        private readonly List<SwaggerDefinition> _swaggerDefinitions = [];
        private SwaggerResponse[] _swaggerResponses = [];
        private readonly Dictionary<string, NotificationSwaggerDefinition> _swaggernotification = [];
        private readonly Dictionary<string, TopicTypeInfo> _topicTypeMap = [];
        private string? _swagger;
        private string? _basePath;
        private string? _host;
        private ApiInfo? _info;

        private readonly List<DefinitionModel> _models = [];
        private readonly List<DefinitionModel> _notificationModels = [];
        private readonly Dictionary<string, List<ApiOperation>> _apis = [];
        [GeneratedRegex(@"\.([a-z0-9]*)\.(\{id\})")]
        private static partial Regex NotificationRegex();

        public ApiGenerator() {
            //_namespace = targetNamespace;
            //if (!Directory.Exists(targetFolder)) {
            //    Directory.CreateDirectory(targetFolder);
            //}
            //_targetFolder = targetFolder;
        }
        public ApiDefaults ApiDefaults { get => _apiDefaults; }
        public void WriteDefinitionsJson(string build) {
            JsonSerializerOptions jsonOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true };
            if (_models.Count != 0) {
                File.WriteAllText($"models-{build}.json", JsonSerializer.Serialize(_models, jsonOptions));
            }
            if (_apis.Count != 0) {
                File.WriteAllText($"apis-{build}.json", JsonSerializer.Serialize(_apis, jsonOptions));
            }
            if (_notificationModels.Count != 0) {
                File.WriteAllText($"notifications-{build}.json", JsonSerializer.Serialize(_notificationModels, jsonOptions));
            }
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
                        case "swagger":
                            // TODO: Check swagger version
                            _swagger = o.Value?.ToString();
                            break;
                        case "info":
                            if (o.Value is JsonObject object1) {
                                _info = new ApiInfo(object1);
                            }
                            break;
                        case "host":
                            _host = o.Value?.ToString();
                            break;
                        case "basePath":
                            _basePath = o.Value?.ToString();
                            break;
                        case "schemes":
                            if (o.Value is JsonArray) {
                                List<string> l = [];
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Schemes = [.. l];
                            }
                            else {
                                throw new Exception("Schemes must be a JsonArray");
                            }
                            break;
                        case "consumes":
                            if (o.Value is JsonArray) {
                                List<string> l = [];
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Consumes = [.. l];
                            }
                            else {
                                throw new Exception("Consumes must be a JsonArray");
                            }

                            break;
                        case "produces":
                            if (o.Value is JsonArray) {
                                List<string> l = [];
                                foreach (var item in o.Value.AsArray()) {
                                    if (item != null) {
                                        l.Add(item.ToString());
                                    }
                                }
                                _apiDefaults.Produces = [.. l];
                            }
                            else {
                                throw new Exception("Produces must be a JsonArray");
                            }

                            break;
                        case "paths":
                            if (o.Value is JsonObject) {
                                foreach (var p in o.Value.AsObject()) {
                                    if (p.Value != null && p.Value is JsonObject) {
                                        if (p.Key.StartsWith('/')) {
                                            ParseSwaggerOperation(p.Key, p.Value.AsObject());
                                            //var api = new SwaggerApi(p.Key, p.Value.AsObject());
                                            //_swaggerApis.Add(api);
                                        }
                                        else {
                                            throw new Exception("Unknown path " + p.Key);
                                        }
                                    }
                                    else {
                                        throw new Exception("Path must not be an array");
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
                                    
                                    if (p.Value != null && p.Value is JsonObject object2) {
                                        _swaggerDefinitions.Add(new SwaggerDefinition(p.Key, object2));
                                    }
                                    else {
                                        throw new Exception("Definition value must be an object");
                                    }
                                }
                            }
                            else {
                                throw new Exception("definition as Array???");
                            }
                            break;
                        case "parameters":
                            // TODO: default definiton parameters
                            throw new NotImplementedException("Parameters are not implemented yet");
                            //break;
                       case "responses":
                            if (o.Value is JsonObject) {
                                List<SwaggerResponse> l = [];
                                foreach (var r in o.Value.AsObject()) {
                                    if (r.Value is JsonObject responsesObject)
                                        l.Add(new SwaggerResponse(r.Key, responsesObject));
                                }
                                _swaggerResponses = [.. l];
                            }
                            else {
                                throw new Exception("Responses must be a JsonObject");
                            }
                            break;
                        case "securityDefinitions":
                            if (o.Value is JsonObject) {
                                List<SecurityScheme> l = [];
                                foreach (var item in o.Value.AsObject()) {
                                    if (item.Value is JsonObject object2) {
                                        l.Add(ParseSecuritySchemeObject(item.Key, object2));
                                    }
                                }
                                _apiDefaults.SecuritySchemes = [.. l];
                            }
                            else {
                                throw new Exception("SecuritySchemes must be a JsonArray");
                            }
                            break;
                        case "security":
                            // TODO: security object
                            throw new NotImplementedException("Security are not implemented yet");
                        case "tags":
                            if (o.Value is JsonArray) {
                                List<TagDescription> l = [];
                                foreach (var item in o.Value.AsArray()) {
                                    if (item is JsonObject) {
                                        l.Add(ParseTag(item.AsObject()));
                                    }
                                }
                                _apiDefaults.Tags = [.. l];
                            }
                            else {
                                throw new Exception("Tags must be a JsonArray");
                            }
                            break;
                        case "externalDocs":
                            if (o.Value is JsonObject @object) {
                                _apiDefaults.ExternalDocs = ParseExternalDocs(@object);
                            }
                            else {
                                throw new Exception("ExternalDocs must be a JsonArray");
                            }
                            break;
                        default:
                            Console.WriteLine("TODO: " + o.Key);
                            break;
                    }
                }
            }
        }

        private void ParseSwaggerOperation(string path, JsonObject o) {
            foreach (var item in o) {
                switch (item.Key) {
                    case "get":
                        if (item.Value is JsonObject getObject) {
                            var op = new SwaggerOperation(path, "Get", getObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "post":
                        if (item.Value is JsonObject postObject) {
                            var op = new SwaggerOperation(path, "Post", postObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "put":
                        if (item.Value is JsonObject putObject) {
                            var op = new SwaggerOperation(path, "Put", putObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "patch":
                        if (item.Value is JsonObject patchObject) {
                            var op = new SwaggerOperation(path, "Patch", patchObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "delete":
                        if (item.Value is JsonObject deleteObject) {
                            var op = new SwaggerOperation(path, "Delete", deleteObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "head":
                        if (item.Value is JsonObject headObject) {
                            var op = new SwaggerOperation(path, "Head", headObject);
                            if (op.Tags?.Length > 0) {
                                foreach (var t in op.Tags) {
                                    if (_swaggerapis.TryGetValue(t, out List<SwaggerOperation>? value)) {
                                        value.Add(op);
                                    }
                                    else {
                                        _swaggerapis.Add(t, [op]);
                                    }
                                }
                            }
                            else {
                                throw new Exception($"No tag found in operation {op.Path}");
                            }
                            //_operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "$ref":
                        // TODO: path.$ref
                        throw new NotImplementedException("$ref in path object not implemented yet");
                    default:
                        Console.WriteLine(item.Key);
                        break;
                }
            }
        }
        private static SecurityScheme ParseSecuritySchemeObject(string key, JsonObject value) {
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
                        throw new Exception($"Unknown {item.Key} paramter in security scheme");
                }
            }
            switch (schemeType) {
                case "basic":
                    return new BasicSecurityScheme(key) { Description = description };
                case "apiKey":
                    if (string.IsNullOrEmpty(parameterName)) {
                        throw new Exception("Missing name in SecurityScheme");
                    }
                    if (string.IsNullOrEmpty(in_)) {
                        throw new Exception("Missing \"in\" in SecurityScheme");
                    }
                    return new ApiKeySecurityScheme(parameterName, in_, key);
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
                    return new OAuth2SecurityScheme(flow, authorizationUrl, tokenUrl, scopes, key);
                default:
                    throw new Exception("Unknown security scheme type " + schemeType);
            }
        }
        private static List<Scope> ParseScopes(JsonObject scopes) {
            List<Scope> scopesList = [];
            foreach (var item in scopes.AsObject()) {
                if (item.Value == null) {
                    throw new Exception("Description of scope must not be empty");
                }
                scopesList.Add(new Scope(item.Key, item.Value.ToString()));
            }
            return scopesList;
        }
        private static TagDescription ParseTag(JsonObject tag) {
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
        private static Documentation ParseExternalDocs(JsonObject externalDocs) {
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

        public void ParseNotificationSwagger(string notificationFile) {
            if (!File.Exists(notificationFile)) {
                throw new ArgumentException("Could not find file \"" + notificationFile + "\"");
            }
            AvailableTopicEntityListing? topicListing;
            try {
                topicListing = JsonSerializer.Deserialize<AvailableTopicEntityListing>(File.ReadAllText(notificationFile));
            }
            catch (Exception ex) {
                throw new Exception($"Error parsing file \"{notificationFile}\"", ex);
            }
            if (topicListing == null) {
                throw new Exception($"Error parsing file \"{notificationFile}\"");
            }
            if(topicListing.Entities?.Any() == true) {
                foreach (var item in topicListing.Entities.Where(p => p.Transports != null && (p.Transports.Contains(AvailableTopic.TransportsConstant.Websocket) || p.Transports.Contains(AvailableTopic.TransportsConstant.All)))) {
                    ParseNotification(item);
                }
            }
            
            else {
                throw new Exception($"No entities found in {notificationFile}.");
            }
        }

        private void ParseNotification(AvailableTopic o) {
            //string? name = null;
            //JsonObject? schema = null;
            //string? description = null;
            //string? visibility = null;
            //foreach (var item in o.AsObject()) {
            //    switch (item.Key) {
            //        case "id":
            //            name = item.Value?.ToString();
            //            break;
            //        case "schema":
            //            schema = item.Value as JsonObject;
            //            break;
            //        case "description":
            //            description = item.Value?.ToString();
            //            break;
            //        case "visibility":
            //            visibility = item.Value?.ToString();
            //            break;
            //        default:
            //            break;
            //    }
            //}
            if(string.IsNullOrEmpty(o.Id) || o.Schema == null || o.Transports == null) {
                throw new Exception("Error reading notification");
            }
            if (o.Schema != null) {
                //foreach(var x in ((JsonElement)o.Schema).EnumerateObject()) {
                //    Console.WriteLine();
                //}
                var nd = ParseNotificationSwaggerDefinition((JsonObject)o.Schema);
                var match = NotificationRegex().Matches(o.Id);
                string[]? topicParameters = null;
                if (match.Count != 0) {
                    var list = match.Select(p => {
                        var x = p.Groups[1].Value;
                        if (x.EndsWith('s')) {
                            x = x.Substring(0, x.Length - 1);
                        }
                        return string.Concat(x[..1].ToUpper(), x.AsSpan(1), "Id");
                    });

                    // Console.WriteLine($"{o.Id} : {string.Join(" | ", list)}");

                    nd.TopicParameters = list.ToArray();
                    topicParameters = nd.TopicParameters;
                }
                _topicTypeMap.Add(o.Id, new TopicTypeInfo(nd.Name, topicParameters, o.TopicParameters?.ToArray(), o.Transports, o.Description));
                _swaggernotification.Add(o.Id, nd);
            }
        }
        NotificationSwaggerDefinition ParseNotificationSwaggerDefinition(JsonObject o) {
            string? id = null;
            string? name = null;
            string? type = null;
            List<NotificationProperty> properties = [];
            string[]? required = null;
            string? description = null;
            foreach (var item in o) {
                switch (item.Key) {
                    case "id":
                        id = item.Value?.ToString();
                        break;
                    case "type":
                        type = item.Value?.ToString();
                        break;
                    case "properties":
                        if (item.Value != null) {
                            foreach (var prop in item.Value.AsObject()) {
                                if (prop.Value != null) {
                                    properties.Add(ParseNotificationProperty(prop.Key, prop.Value.AsObject()));
                                }
                            }
                        }
                        break;
                    case "required":
                        if (item.Value is JsonArray) {
                            var list = new List<string>();
                            foreach (var prop in item.Value.AsArray()) {
                                if (prop != null) {
                                    list.Add(prop.ToString());
                                }
                            }
                            required = [.. list];
                        }
                        break;
                    case "description":
                        description = item.Value?.ToString();
                        break;
                    default:
                        Console.WriteLine(item.Key);
                        break;
                }
            }
            if (!string.IsNullOrEmpty(id)) {
                string[] n = id.Replace("urn:jsonschema:", "").Split(':');
                name = string.Join("", n.Select(p => string.Concat(p[..1].ToUpper(), p.AsSpan(1))));
            }
            else {
                throw new Exception("No id found for notification.");
            }
            //Console.WriteLine("Parse " + id);
            return new NotificationSwaggerDefinition(id, type, [.. properties], required, description);

        }
        NotificationProperty ParseNotificationProperty(string? name_, JsonObject value) {
            string? name = name_;
            string? description = null;
            bool? readOnly = null;
            bool? uniqueItems = null;
            string? example = null;
            string? genesysEntityType = null;
            string[]? genesysSearchFields = null;
            double? minimum = null;
            double? maximum = null;
            int? minItems = null; 
            int? maxItems = null;
            int? minLength = null;
            int? maxLength = null;
            int? position = null;
            NotificationProperty[]? properties = null;
            NotificationBase b = ParseNotificationBase(value);
            foreach (var item in value) {
                switch (item.Key) {
                    case "name":
                        if (item.Value != null) {
                            name = item.Value?.ToString();
                        }
                        else {
                            throw new Exception("Name must not be null");
                        }
                        break;
                    case "description":
                        description = item.Value?.ToString();
                        break;

                    case "readOnly":
                        readOnly = item.Value?.GetValue<bool>();
                        break;
                    case "uniqueItems":
                        uniqueItems = item.Value?.GetValue<bool>();
                        break;
                    case "example":
                        example = item.Value?.ToString();
                        break;
                    case "x-genesys-entity-type":
                        genesysEntityType = item.Value?.ToString();
                        break;
                    case "x-genesys-search-fields":
                        if (item.Value is JsonArray) {
                            var list = new List<string>();
                            foreach (var prop in item.Value.AsArray()) {
                                if (prop != null) {
                                    list.Add(prop.ToString());
                                }
                            }
                            genesysSearchFields = [.. list];
                        }
                        break;
                    case "minimum":
                        minimum = item.Value?.GetValue<double>();
                        break;
                    case "maximum":
                        maximum = item.Value?.GetValue<double>();
                        break;
                    case "maxItems":
                        maxItems = item.Value?.GetValue<int>();
                        break;
                    case "minItems":
                        minItems = item.Value?.GetValue<int>();
                        break;
                    case "minLength":
                        minLength = item.Value?.GetValue<int>();
                        break;
                    case "maxLength":
                        maxLength = item.Value?.GetValue<int>();
                        break;
                    case "position":
                        position = item.Value?.GetValue<int>();
                        break;
                    case "properties":
                        List<NotificationProperty> l = [];
                        if(item.Value is JsonObject) {
                            foreach (var p in item.Value.AsObject()) {
                                if (p.Value is JsonObject) {
                                    l.Add(ParseNotificationProperty(p.Key, p.Value.AsObject()));
                                }
                            }
                            properties = [.. l];
                        }
                        break;
                    // Handled in SwaggerBase
                    case "id":
                    case "$ref":
                    case "type":
                    case "format":
                    case "enum":
                    case "items":
                    case "additionalProperties":
                        break;
                    default:
                        break;
                        //throw new Exception($"Unknown key {item.Key} in property");

                }
            }
            if (string.IsNullOrEmpty(name)) {
                throw new Exception("Name must not be null");
            }
            var n = new NotificationProperty(name, description, properties, readOnly, uniqueItems, example, genesysEntityType, genesysSearchFields, minimum, maximum, minItems, maxItems, minLength, maxLength, position, b);
            if (!string.IsNullOrEmpty(n.Id) && n.Type == "object") {
                var t = n.GetTypeInfo(n.Id);
                //if (_notificationModels.FirstOrDefault(p => p.Id == n.Id) != null) {
                //    Console.WriteLine(n.Id);
                //}
                DefinitionModel? model = null;
                 if (n.Properties != null) {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null));
                }
                else {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, "System.Object", null));

                }
                var check = _notificationModels.FirstOrDefault(p => p.Id == n.Id);
                if(check != null) { 
                    if(check.Properties.Count < model.Properties.Count) {
                        check.Properties = model.Properties;
                    }
                }
                else {
                    _notificationModels.Add(model);
                }

            }
            //if (!string.IsNullOrEmpty(n.Id) && n.Properties != null) {
            //    var t = n.GetTypeInfo(n.Id);

            //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null)));
            //}
            return n;

            //return new NotificationProperty(name, description, readOnly, uniqueItems, example, genesysEntityType, genesysSearchFields, minimum, maximum, minItems, maxItems, minLength, maxLength, position, b);
        }

        NotificationBase ParseNotificationBase(JsonObject o) {
            string? id = null;
            string? @ref = null;
            string? type = null;
            string? format = null;
            NotificationArray? items = null;
            string[]? enumValues = null;
            NotificationAdditionalProperties? additionalProperties = null;
            string? @default = null;
            foreach (var item in o) {
                switch (item.Key) {
                    case "id":
                        id = item.Value?.ToString();
                        break;
                    case "$ref":
                        @ref = item.Value?.ToString();
                        break;
                    case "type":
                        type = item.Value?.ToString();
                        break;
                    case "enum":
                        if (item.Value is JsonArray) {
                            var list = new List<string>();
                            foreach (var prop in item.Value.AsArray()) {
                                if (prop != null) {
                                    list.Add(prop.ToString());
                                }
                            }
                            enumValues = [.. list];
                        }
                        break;
                    case "format":
                        format = item.Value?.ToString();
                        break;
                    case "items":
                        if (item.Value is JsonObject @object)
                            items = ParseNotificationArray(@object);
                        break;
                    case "additionalProperties":
                        if (item.Value is JsonObject object1) {
                            additionalProperties = ParseNotificationAdditionalProperties(object1);
                        }
                        break;
                    case "default":
                        @default = item.Value?.ToString();
                        break;
                }
            }
            if (type == "array" && items == null) {
                throw new Exception("Array must have Items");
            }


            return new NotificationBase(id, @ref, type, format, items, enumValues, additionalProperties, @default);
        }
        NotificationAdditionalProperties ParseNotificationAdditionalProperties(JsonObject o) {
            bool? uniqueItems = null;
            NotificationProperty[]? properties = null;
            NotificationBase b = ParseNotificationBase(o);
            foreach (var item in o) {
                switch (item.Key) {
                    case "uniqueItems":
                        uniqueItems = item.Value?.GetValue<bool>();
                        break;
                    case "properties":
                        if (item.Value != null) {
                            List<NotificationProperty> l = [];
                            foreach (var prop in item.Value.AsObject()) {
                                if (prop.Value != null) {
                                    l.Add(ParseNotificationProperty(prop.Key, prop.Value.AsObject()));
                                }
                            }
                            properties = [.. l];
                        }
                        break;
                    // Handled in NotificationBase
                    case "id":
                    case "$ref":
                    case "type":
                    case "format":
                    case "items":
                    case "enum":
                        break;
                    default:
                        Console.WriteLine($"case \"{item.Key}\":\r\n\tbreak;");
                        break;
                }
            }
            var n = new NotificationAdditionalProperties(uniqueItems, properties, b);
            if (!string.IsNullOrEmpty(n.Id) && n.Type == "object") {
                var t = n.GetTypeInfo(n.Id);
                //if (_notificationModels.FirstOrDefault(p => p.Id == n.Id) != null) {
                //    Console.WriteLine(n.Id);
                //}
                DefinitionModel? model = null;
                if (n.Properties != null) {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null));
                }
                else {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, "System.Object", null));

                }
                var check = _notificationModels.FirstOrDefault(p => p.Id == n.Id);
                if (check != null) {
                    if (check.Properties.Count < model.Properties.Count) {
                        check.Properties = model.Properties;
                    }
                }
                else {
                    _notificationModels.Add(model);
                }

                //if (_notificationModels.FirstOrDefault(p => p.Id == n.Id) != null) {
                //    Console.WriteLine(n.Id);
                //}
                //if (n.Properties != null) {
                //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null)));
                //}
                //else {
                //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, "System.Object", null)));

                //}
            }
            //if (!string.IsNullOrEmpty(n.Id) && n.Properties != null) {
            //    var t = n.GetTypeInfo(n.Id);

            //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null)));
            //}

            return n;
        }

        NotificationArray ParseNotificationArray(JsonObject o) {
            string? description = null;
            NotificationProperty[]? properties = null;
            NotificationBase b = ParseNotificationBase(o);
            foreach (var item in o) {
                switch (item.Key) {
                    case "description":
                        description = item.Value?.ToString();
                        break;
                    case "properties":
                        if (item.Value != null) {
                            List<NotificationProperty> l = [];
                            foreach (var prop in item.Value.AsObject()) {
                                if (prop.Value != null) {
                                    l.Add(ParseNotificationProperty(prop.Key, prop.Value.AsObject()));
                                }
                            }
                            properties = [.. l];
                        }
                        break;
                    // Handled in NotificationBase
                    case "id":
                    case "$ref":
                    case "type":
                    case "enum":
                    case "format":
                    case "additionalProperties":
                    case "default":
                        break;
                    default:
                        throw new Exception($"Unknown key {item.Key} in item array");
                }
            }
            var n = new NotificationArray(description, properties, b);
            if (!string.IsNullOrEmpty(n.Id) && n.Type == "object") {
                var t = n.GetTypeInfo(n.Id);
                if (_notificationModels.FirstOrDefault(p => p.Id == n.Id) != null) {
                    Console.WriteLine(n.Id);
                }

                DefinitionModel? model = null;
                if (n.Properties != null) {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null));
                }
                else {
                    model = new DefinitionModel(new NotificationSwaggerDefinition(n.Id, "System.Object", null));

                }
                var check = _notificationModels.FirstOrDefault(p => p.Id == n.Id);
                if (check != null) {
                    if (check.Properties.Count < model.Properties.Count) {
                        check.Properties = model.Properties;
                    }
                }
                else {
                    _notificationModels.Add(model);
                }

                //if (_notificationModels.FirstOrDefault(p => p.Id == n.Id) != null) {
                //    Console.WriteLine(n.Id);
                //}
                //if (n.Properties != null) {
                //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null)));
                //}
                //else {
                //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, "System.Object", null)));

                //}
            }
            //if (!string.IsNullOrEmpty(n.Id) && n.Properties != null) {
            //    var t = n.GetTypeInfo(n.Id);

            //    _notificationModels.Add(new DefinitionModel(new NotificationSwaggerDefinition(n.Id, t.TypeName, n.Properties, null, null)));
            //}
            return n;

        }

        public void CreateNotificationDefinitions() {
            foreach (var item in _swaggernotification) {
                DefinitionModel def = new(item.Value);
                var check = _notificationModels.FirstOrDefault(p => p.Name == def.Name);
                if (check != null) {
                    //var result = check.Properties.ExceptBy(def.Properties.Select(x => x.Name), x => x.Name);
                    //if (result.Count() > 0) {
                    //    Console.WriteLine();
                    //}
                }
                else {
                    _notificationModels.Add(def);
                }
            }
            foreach (var item in _notificationModels) {
                foreach (var prop in item.Properties) {
                    var checkAlias = _notificationModels.FirstOrDefault(p => p.Name == prop.TypeName && p.Alias != null); 
                    if(checkAlias != null && checkAlias.Alias != null) {
                        prop.TypeName = checkAlias.Alias;
                    }
                }
            }
        }
        public void CreateApis() {
            foreach (var item in _swaggerapis) {
                _apis.Add(item.Key.Replace(" ","").Replace("&", ""), item.Value.Select(p => CreateApiOperation(p)).ToList());
            }
            //foreach (var item in _swaggerApis) {
            //    string apiName = item.Path.Substring("/api/v2/".Length);
            //    if (apiName.Contains('/')) {
            //        apiName = apiName.Substring(0, apiName.IndexOf("/"));
            //    }
            //    if (_apis.ContainsKey(apiName)) {
            //        _apis[apiName].AddRange(item.Operations.Select(p => CreateApiOperation(p)).ToList());
            //    }
            //    else {
            //        _apis.Add(apiName, item.Operations.Select(p => CreateApiOperation(p)).ToList());
            //    }
            //}

        }
        public void CreateModels() {
            foreach (var item in _swaggerDefinitions) {
                DefinitionModel def = new(item);
                _models.Add(def);
            }
        }

        public void Generate(string targetFolder, string targetNamespace) {
            WritePathsDefinitions(targetFolder, targetNamespace);
            string modelFolder = Path.Combine(targetFolder, "Models");
            if (!Directory.Exists(modelFolder)) {
                Directory.CreateDirectory(modelFolder);
            }
            else {
                //ClearDirectory(modelFolder);
            }

            var modelFiles = Directory.GetFiles(modelFolder, "*.cs").ToList();
            List<string> newFiles =
            [
                .. WriteDataDefinitions(modelFolder, targetNamespace),
                .. WriteNotificationDefinitions(modelFolder, targetFolder, targetNamespace),
            ];
            // Remove old files
            var oldFiles = modelFiles.Except(newFiles);
            foreach (var item in oldFiles) {
                File.Delete(item);
            }

        }

        private List<string> WriteDataDefinitions(string modelFolder, string targetNamespace) {
            int i = 0;
            var modelsFile = Directory.GetFiles(".", "models-*.json").OrderByDescending(p => p).FirstOrDefault() ?? throw new Exception("Error reading models");
            var models = JsonSerializer.Deserialize<List<DefinitionModel>>(File.ReadAllText(modelsFile)) ?? throw new Exception("Error parsing models");
            Console.Write("Writing models ");
            List<string> files = [];
            foreach (var def in models) {
                if (string.IsNullOrEmpty(def.Name)) {
                    throw new Exception("Name of data object must not be null");
                }
                if (++i % 100 == 0) {
                    Console.Write(".");
                }
                var filename = Path.Combine(modelFolder, def.Name + ".cs");
                files.Add(filename);
                using var writer = new StreamWriter(filename);
                writer.WriteLine($"using {targetNamespace};");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Runtime.Serialization;");
                writer.WriteLine("using System.Text.Json.Serialization;");
                writer.WriteLine();
                writer.WriteLine($"namespace {targetNamespace}.Models {{");
                WriteModelDefinition(def, writer, 1);
                writer.WriteLine("}");

            }
            Console.WriteLine();
            Console.WriteLine(i + " models written.");
            return files;
        }
        private List<string> WriteNotificationDefinitions(string modelFolder, string targetFolder, string targetNamespace) {
            int i = 0;
            var notificationsFile = Directory.GetFiles(".", "notifications-*.json").OrderByDescending(p => p).FirstOrDefault() ?? throw new Exception("Error reading notifications");
            var notificationModels = JsonSerializer.Deserialize<List<DefinitionModel>>(File.ReadAllText(notificationsFile)) ?? throw new Exception("Error parsing notification models");
            var alias = notificationModels.Where(p => p.Alias != null).ToArray();
            Console.Write("Writing notification models ");
            List<string> files = [];
            foreach (var def in notificationModels.Where(p => p.Alias == null)) {
                if (string.IsNullOrEmpty(def.Name)) {
                    throw new Exception("Name of data object must not be null");
                }
                if (++i % 100 == 0) {
                    Console.Write(".");
                }
                var filename = Path.Combine(modelFolder, def.Name + ".cs");
                files.Add(filename);
                using var writer = new StreamWriter(filename);
                writer.WriteLine($"using {targetNamespace};");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Runtime.Serialization;");
                writer.WriteLine("using System.Text.Json.Serialization;");
                writer.WriteLine();
                //def.Properties.Select(p => p.TypeName).ToList().ForEach(p => {
                //    var a = alias.FirstOrDefault(a => a.Name == p);
                //    if(a != null) {
                //        writer.WriteLine($"using {p} = {a.Alias};");
                //    }
                //});
                //if (def.IsAlias) {
                //    writer.WriteLine($"using {def.Name} = {def.T}
                //}
                writer.WriteLine($"namespace {targetNamespace}.Models {{");
                WriteModelDefinition(def, writer, 1);
                writer.WriteLine("}");

            }
            Console.WriteLine();
            Console.WriteLine(i + " notification models written.");
            using (var writer = new StreamWriter(Path.Combine(targetFolder, "NotificationChannelTopicMap.cs"))) {
                writer.WriteLine($"using {targetNamespace}.Models;");
                writer.WriteLine();
                writer.WriteLine($"namespace {targetNamespace} {{");
                writer.WriteIndent(1).WriteLine("public partial class NotificationChannel {");
                writer.WriteIndent(2).WriteLine("static private readonly Dictionary<string, TopicTypeInfo> _topicTypeMap = new() {");
                foreach (var item in _topicTypeMap) {
                    writer.WriteIndent(3).Write($"{{ \"{item.Key}\", new TopicTypeInfo(typeof({item.Value.TypeName}),  ");
                    if (item.Value.TopicParameters?.Length > 0) {
                        writer.WriteLine($"new string[] {{ {string.Join(", ", item.Value.TopicParameters.Select(p => $"\"{p}\""))} }}) }},");
                    }
                    else {
                        writer.WriteLine("Array.Empty<string>()) },");
                    }
                }
                writer.WriteIndent(2).WriteLine("};");
                writer.WriteIndent(1).WriteLine("}");
                writer.WriteLine("}");
            }
            return files;
        }
        private ApiOperation CreateApiOperation(SwaggerOperation op) {
            if (op.Responses != null) {
                foreach (SwaggerResponse item in _swaggerResponses) {
                    if (op.Responses.SingleOrDefault(p => p.Name == item.Name) == null) {
                        op.Responses = [item, .. op.Responses];
                    }
                }
            }
            else {
                op.Responses = _swaggerResponses;
            }
            var operation = new ApiOperation() {
                Id = op.OperationId,
                IsDeprecated = op.Deprecated,
                Responses = op.Responses?.Select(p => CreateApiResponse(op.OperationId, p)),
                Method = op.Method,
                PurecloudMethodName = op.PurecloudMethodName,
                Produces = op.Produces,
                Consumes = op.Consumes,

                Parameters = op.Parameters?.Select(p => CreateApiParameter(p)).ToList(),
                Path = op.Path,
                Summary = (op.Summary ?? op.Description) ?? "",
                Description = op.Summary != null ? op.Description ?? "" : "",
                Tags = op.Tags,
                Permissions = op.Security?.Select(p => new ApiPermisson() { PermissionType = p.Key, Permissions = p.Value })
            };

            

            return operation;
        }
        private static ApiOperationParameter CreateApiParameter(SwaggerParameter p) {
            ApiOperationParameter param = new() {
                Name = p.Name
            };
            if (p.Ref != null) {
                param.TypeName = p.Ref.Replace("#/definitions/", "");
            }
            else if (p.Type != null) {
                TypeInfo t = p.GetTypeInfo(param.PName);
                param.TypeName = t.TypeName;
                param.IsCollection = t.IsCollection;
                if (t.EnumModel != null) {
                    // Workaround for wildcard enums
                    if (!t.EnumModel.EnumValues.Keys.Any(p => p.Contains('*'))) {
                        param.EnumValues = t.EnumModel.EnumValues;
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
        private static ApiOperationResponse CreateApiResponse(string operationName, SwaggerResponse r) {
            ApiOperationResponse response = new();
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

        private void WritePathsDefinitions(string targetFolder, string targetNamespace) {
            string apiFolder = Path.Combine(targetFolder, "Api");
            if (!Directory.Exists(apiFolder)) {
                Directory.CreateDirectory(apiFolder);
            }
            int i = 0;
            Console.WriteLine("Writing apis ");
            var apiFile = Directory.GetFiles(".", "apis-*.json").OrderByDescending(p => p).FirstOrDefault() ?? throw new Exception("Error reading notifications");
            var apis = JsonSerializer.Deserialize<Dictionary<string, List<ApiOperation>>>(File.ReadAllText(apiFile)) ?? throw new Exception("Error parsing notification models");
            var apiFiles = Directory.GetFiles(apiFolder, "*.cs").ToList();
            List<string> newFiles = new();

            //var groups = _apis.Select(p => p.Name).Distinct().OrderBy(p => p);
            foreach (var group in apis) {
                if (++i % 10 == 0) {
                    Console.Write(".");
                }

                //if (!path.Name.StartsWith(group)) {
                string groupName = CreateName(group.Key);
                var filename = Path.Combine(apiFolder, groupName + "Api.cs");
                newFiles.Add(filename);
                StreamWriter? writer = new(filename);
                writer.WriteLine($"using {targetNamespace}.Models;");
                writer.WriteLine("using Microsoft.AspNetCore.Http.Extensions;");
                writer.WriteLine("using Microsoft.Extensions.Logging;");
                writer.WriteLine("using Microsoft.Extensions.Logging.Abstractions;");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Linq;");
                writer.WriteLine("using System.Net.Http;");
                writer.WriteLine("using System.Net.Http.Headers;");
                writer.WriteLine("using System.Net.Http.Json;");
                writer.WriteLine("using System.Text.Json.Serialization;");
                writer.WriteLine("using System.Threading.Tasks;");
                writer.WriteLine();
                writer.WriteLine($"namespace {targetNamespace} {{");
                writer.WriteLine($"\tpublic class {groupName}Api {{");
                writer.WriteLine("\t\tprivate readonly ConnectionManager _connectionManager;");
                writer.WriteLine("\t\tprivate readonly ILogger _logger;");
                writer.WriteLine("\t\t/// <summary>");
                writer.WriteLine($"\t\t/// Initializes a new instance of the <see cref=\"{groupName}Api\"/> class.");
                writer.WriteLine("\t\t/// </summary>");
                writer.WriteLine("\t\t/// <param name=\"connectionManager\">An instance of <see cref=\"ConnectionManager>\"/></param>");
                writer.WriteLine("\t\t/// <param name=\"logger\">An intance of a logger.</param>");
                writer.WriteLine("\t\t/// <returns></returns>");
                writer.WriteLine($"\t\tpublic {groupName}Api(ConnectionManager connectionManager, ILogger<{groupName}Api>? logger = null) {{");
                writer.WriteLine("\t\t\t_connectionManager= connectionManager;");
                writer.WriteLine($"\t\t\t_logger = logger ?? NullLogger<{groupName}Api>.Instance;");
                writer.WriteLine("\t\t}");

                foreach (var op in group.Value) {
                    WriteOperation(op, writer, 2);
                }
                writer.WriteLine("\t}");
                writer.WriteLine("}");
                writer.Close();
            }
            Console.WriteLine();
            // Remove old files
            var oldFiles = apiFiles.Except(newFiles);
            foreach (var item in oldFiles) {
                File.Delete(item);
            }

            Console.WriteLine(i + " apis written.");
        }
        private void WriteOperation(ApiOperation operation, StreamWriter writer, int indent) {
            string operationName = $"{operation.Id.Substring(0, 1).ToUpper()}{operation.Id.Substring(1)}";
            if (operation.Parameters?.Any() == true) {
                foreach (var item in operation.Parameters.Where(p => p.EnumValues != null)) {
                    WriteEnumDefinition(operationName + item.TypeName, item.EnumValues, writer, 2);
                    //writer.WriteIndent(indent + 1).WriteLine($"public enum {operationName + item.TypeName} {{ {string.Join(",", item.EnumValues)} }}");
                }
            }
            var enumResponse = operation.Responses?.Where(p => p.EnumModel != null).FirstOrDefault();
            if (enumResponse != null && enumResponse.EnumModel != null) {
                WriteEnumDefinition(enumResponse.TypeName, enumResponse.EnumModel.EnumValues, writer, 2);
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
                        if (item.Length > 0) {
                            writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                        }
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
            if (operation.IsDeprecated) {
                writer.WriteIndent(indent + 1).WriteLine("[Obsolete]");
            }
            // TODO: check successful response
            string response = "void";
            if (operation.Responses != null) {
                var res = operation.Responses.FirstOrDefault(p => p.ResponseCode == "default");
                res ??= operation.Responses.OrderBy(p => p.ResponseCode).First();
                if (res.TypeName != null) {
                    if (res.IsDictionary && res.IsCollection) {
                        response = $"Dictionary<string,IEnumerable<{res.TypeName}>>";
                    }
                    else if (res.IsDictionary) {
                        response = $"Dictionary<string,{res.TypeName}>";
                    }
                    else if (res.IsCollection) {
                        response = $"IEnumerable<{res.TypeName}>";
                    }
                    else {
                        response = res.TypeName;
                    }
                }
                else {
                    //Console.WriteLine(res.Name + " without type");
                }

            }
            if (response == "void") {
                writer.WriteIndent(indent + 1).Write("public async Task ");

            }
            else {
                if (response == "Action") { // Workaround for ambigous System.Action
                    writer.WriteIndent(indent + 1).Write($"public async Task<Models.{response}> ");
                }
                else {
                    writer.WriteIndent(indent + 1).Write($"public async Task<{response}> ");
                }
            }
            writer.Write($"{operationName} (");
            if (operation.Parameters?.Any() == true) {
                writer.Write(string.Join(", ", operation.Parameters.OrderByDescending(p => p.IsRequired).Select(p => {
                    var typeName = p.TypeName;
                    if (p.EnumValues != null) {
                        typeName = $"{operationName}{typeName}";
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
            // TODO: Check query parameter
            if (operation.Parameters != null) {
                var queryParams = operation.Parameters?.Where(p => p.Position == ApiOperationParameter.ParameterKind.Query);
                if (queryParams?.Any() == true) {
                    writer.WriteIndent(indent + 2).WriteLine("// Query params");
                    writer.WriteIndent(indent + 2).WriteLine("var q_ = new QueryBuilder();");

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
                                    writer.WriteIndent(indent + 2).WriteLine($"if({item.PName}?.Any() == true) {{");
                                    writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"{item.Name}\", string.Join(\",\",{item.PName}.Select(p => p.GetAttribute<JsonEnumNameAttribute>().JsonName)));");
                                    writer.WriteIndent(indent + 2).WriteLine("}");
                                }
                                else {
                                    writer.WriteIndent(indent + 2).WriteLine($"q_.Add(\"{item.Name}\", {item.PName}.GetAttribute<JsonEnumNameAttribute>().JsonName);");
                                }
                            }
                            else {
                                if (item.IsCollection) {
                                    writer.WriteIndent(indent + 2).WriteLine($"if({item.PName}?.Any() == true) {{");
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
                                writer.WriteIndent(indent + 2).WriteLine($"q_.Add(\"{item.Name}\", (({item.TypeName}){item.PName}).ToString(\"yyyy-MM-ddTHH:mm:ssZ\"));");
                             }
                            else {
                                writer.WriteIndent(indent + 2).WriteLine($"if({item.PName}.HasValue && !string.IsNullOrEmpty({item.PName}.ToString())) {{");
                                writer.WriteIndent(indent + 3).WriteLine($"q_.Add(\"({item.Name}\", (({item.TypeName}){item.PName}).ToString(\"yyyy-MM-ddTHH:mm:ssZ\"));");
                                writer.WriteIndent(indent + 2).WriteLine("}");
                            }
                        }
                        else if (item.IsCollection) {
                            writer.WriteIndent(indent + 2).WriteLine($"if({item.PName}?.Any() == true) {{");
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
            var otherParams = operation.Parameters?.Where(p => p.Position != ApiOperationParameter.ParameterKind.Path && p.Position != ApiOperationParameter.ParameterKind.Query && p.Position != ApiOperationParameter.ParameterKind.Body);
            if (otherParams != null) {
                foreach (var item in otherParams) {
                    writer.WriteIndent(indent + 2).WriteLine($"// {item.Position}: {item.TypeName} {item.Name}");
                }
            }
            writer.WriteLine();
            writer.WriteIndent(indent + 2).WriteLine("// make the HTTP request");
            //writer.WriteLine("#pragma warning disable IDE0017 // Simplify object initialization");
            writer.WriteIndent(indent + 2).WriteLine("var uri = new UriBuilder(httpClient.BaseAddress) {");
            //writer.WriteLine("#pragma warning restore IDE0017 // Simplify object initialization");
            writer.WriteIndent(indent + 3).WriteLine("Path = requestPath");
            writer.WriteIndent(indent + 2).WriteLine("};");
            if (operation.Parameters?.Any(p => p.Position == ApiOperationParameter.ParameterKind.Query) == true) {
                writer.WriteIndent(indent + 2).WriteLine("uri.Query = q_.ToString();");
            }
            var bodyParam = operation.Parameters?.FirstOrDefault(p => p.Position == ApiOperationParameter.ParameterKind.Body);

            writer.WriteIndent(indent + 2).WriteLine($"using HttpRequestMessage request = new(HttpMethod.{operation.Method}, uri.Uri);");
            if (bodyParam != null) {
                writer.WriteIndent(indent + 2).WriteLine($"using HttpContent content = JsonContent.Create({bodyParam.Name}, new MediaTypeWithQualityHeaderValue(\"application/json\"));");
                writer.WriteIndent(indent + 2).WriteLine("request.Content = content;");
            }
            List<string> consumes = [];
            if(_apiDefaults.Consumes?.Length > 0) {
                consumes.AddRange(_apiDefaults.Consumes);
            }
            if (operation.Consumes?.Any() == true) {
                consumes.AddRange(operation.Consumes);
            }
            foreach (var item in consumes.Distinct()) {
                writer.WriteIndent(indent + 2).WriteLine($"request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(\"{item}\"));");
            }
            
            writer.WriteIndent(indent + 2).WriteLine("using var response = await httpClient.SendAsync(request);");

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
                                if(response == "Action") {// Workaround for ambigous System.Action
                                    response = "Models." + response;
                                }
                                writer.WriteIndent(indent + 3).WriteLine($"var result = await response.Content.ReadFromJsonAsync<{response}>();");
                                if (response != "int" && item.EnumModel == null) {
                                    writer.WriteIndent(indent + 3).WriteLine("return result ?? throw new ApiException(\"{CreateName(operation.Id)} returned empty body\");");
                                }
                                else {
                                    writer.WriteIndent(indent + 3).WriteLine("return result;");
                                }
                            }
                            else {
                                writer.WriteIndent(indent + 3).WriteLine("return;");
                            }
                            writer.WriteIndent(indent + 2).WriteLine("}");
                        }
                    }
                    else if (item.ResponseCode == "default") {
                        writer.WriteIndent(indent + 2).WriteLine($"if (response.IsSuccessStatusCode) {{");
                        if (response != "void") {
                            if (response == "Action") { // Workaround for ambigous System.Action
                                response = "Models." + response;
                            }
                            writer.WriteIndent(indent + 3).WriteLine($"var result = await response.Content.ReadFromJsonAsync<{response}>();");
                            writer.WriteIndent(indent + 3).WriteLine($"return result ?? throw new ApiException(\"{CreateName(operation.Id)} returned empty body\");");
                        }
                        else {
                            writer.WriteIndent(indent + 3).WriteLine("return;");
                        }
                        writer.WriteIndent(indent + 2).WriteLine("}");

                        //if (response != "void") {
                        //    if (response == "Action") { // Workaround for ambigous System.Action
                        //        response = "Models." + response;
                        //    }
                        //    writer.WriteIndent(indent + 2).WriteLine($"var result = await response.Content.ReadFromJsonAsync<{response}>();");
                        //    if (response != "int") {
                        //        writer.WriteIndent(indent + 2).WriteLine("if(result == null) {");
                        //        writer.WriteIndent(indent + 2).WriteLine($"throw new ApiException(\"{CreateName(operation.Id)} returned empty body\");");
                        //        writer.WriteIndent(indent + 2).WriteLine("}");
                        //    }
                        //    writer.WriteIndent(indent + 2).WriteLine("return result;");
                        //}
                        //else {
                        //    Console.WriteLine("Handle default response");
                        //}
                    }
                    else {
                        throw new Exception("Error in writing operation " + operation.Path);
                    }
                }
            }

            writer.WriteIndent(indent + 2).WriteLine($"throw new ApiException($\"Error {{response.StatusCode}} calling {CreateName(operation.Id)}\");");
            writer.WriteIndent(indent + 1).WriteLine("}");
        }



        static void WriteModelDefinition(DefinitionModel model, StreamWriter writer, int indent) {

            if (model.Description != null) {
                writer.WriteIndent(indent + 1).WriteLine("/// <summary>");
                foreach (var item in model.Description.Split("\n")) {
                    writer.WriteIndent(indent + 1).WriteLine($"/// {item}");
                }
                writer.WriteIndent(indent + 1).WriteLine("/// </summary>");
            }
            var notificationbase = model.IsNotification ? ": NotificationEvent " : "";
            writer.WriteIndent(indent + 1).WriteLine($"public class {model.Name} {notificationbase}{{");
            if (model.Properties != null) {
                //foreach (var item in EnumDefinitions) {
                foreach (var item in model.EnumDefinitions) {
                    if (item.EnumValues != null) {
                        WriteEnumDefinition(item.Name, item.EnumValues, writer, indent + 1);
                    }
                }
                writer.WriteLine("#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");
                foreach (var item in model.Properties) {
                    WriteProperty(item, writer, indent + 1);
                }
                if (model.TopicParameters != null) {
                    writer.WriteIndent(indent + 2).WriteLine("// Topic parameter");
                    foreach (var item in model.TopicParameters) {
                        writer.WriteIndent(indent + 2).WriteLine($"public string Notification{item} {{ get; set; }}");
                    }
                }
                writer.WriteLine("#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.\r\n");
            }
            writer.WriteIndent(indent + 1).WriteLine("}");
        }

        static void WriteEnumDefinition(string name, Dictionary<string,string> values, StreamWriter writer, int indent) {
            //StringBuilder sb = new();
            writer.WriteIndent(indent + 1).WriteLine("[JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]");
            writer.WriteIndent(indent + 1).WriteLine($"public enum {name} {{");
            foreach (var item in values) {
                writer.WriteIndent(indent + 2).WriteLine($"[JsonEnumName(\"{item.Key}\")]");
                writer.WriteIndent(indent + 2).WriteLine($"{item.Value},");
            }
            writer.WriteIndent(indent + 1).WriteLine("}");

            //return sb.ToString();
        }

        static void WriteProperty(DefinitionPropertyModel param, StreamWriter writer, int indent) {
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
            if (param.IsCollection && param.IsDictionary) {
                writer.WriteIndent(indent + 1).WriteLine($"public Dictionary<string, IEnumerable<{param.TypeName}>>{notRequired} {param.Name} {{ get; set; }}");
            }
            else if (param.IsDictionary) {
                writer.WriteIndent(indent + 1).WriteLine($"public Dictionary<string, {param.TypeName}>{notRequired} {param.Name} {{ get; set; }}");
            }
            else if (param.IsCollection) {
                writer.WriteIndent(indent + 1).WriteLine($"public IEnumerable<{param.TypeName}>{notRequired} {param.Name} {{ get; set; }}");
            }
            else {
                writer.WriteIndent(indent + 1).WriteLine($"public {param.TypeName}{notRequired} {param.Name} {{ get; set; }}");
            }
            //return sb.ToString();
        }

        [GeneratedRegex("(-[A-Za-z])")]
        private static partial Regex NameRegex();
        [GeneratedRegex("(-[0-9])")]
        private static partial Regex NameNumberRegex();
        [GeneratedRegex("(_[A-Za-z])")]
        private static partial Regex NameToUpperRegex();
        [GeneratedRegex("(/[A-Za-z])")]
        private static partial Regex NameToUpper2Regex();
        [GeneratedRegex("(:[A-Za-z0-9])")]
        private static partial Regex NameToUpper3Regex();
        [GeneratedRegex("(\\$[A-Za-z0-9])")]
        private static partial Regex NameToUpper4Regex();
        [GeneratedRegex("(\\.[A-Za-z0-9])")]
        private static partial Regex NameToUpper5Regex();
        internal static string CreateName(string jsonName, bool lower = false) {
            string name = NameRegex().Replace(jsonName, (p) => p.Value[1..].ToUpper());
            name = NameNumberRegex().Replace(name, (p) => "_" + p.Value[1..]);
            if (!name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                name = NameToUpperRegex().Replace(name, (p) => p.Value[1..].ToUpper());
            }
            name = NameToUpper2Regex().Replace(name, (p) => p.Value[1..].ToUpper());
            name = NameToUpper3Regex().Replace(name, (p) => "_" + p.Value[1..].ToUpper());
            name = NameToUpper4Regex().Replace(name, (p) => p.Value[1..].ToUpper());
            name = NameToUpper5Regex().Replace(name, (p) => p.Value[1..].ToUpper());
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
            if (name.EndsWith('$')) {
                name = name.Substring(0,name.Length-1);
            }
            return name;
        }

    }

    public static class WriterExtensions {
        public static StreamWriter WriteIndent(this StreamWriter writer, int value) {
            if (writer == null) {
                throw new ArgumentException("Writer must not be null", nameof(writer));
            }
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            writer.Write(string.Join("\t", Enumerable.Repeat("", value)));
            return writer;
        }
    }
}