using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerOperation {
        public SwaggerOperation(string path, string method, JsonObject o) {
            Path = path;
            Method = method;
            string? operationId = null;
            foreach (var item in o) {
                switch (item.Key) {
                    case "tags":
                        if(item.Value is JsonArray) {
                            List<string> tags = new();
                            foreach (var t in item.Value.AsArray()) {
                                if (t != null) {
                                    tags.Add(t.ToString());
                                }
                            }
                            Tags = tags.ToArray();
                        }
                        else {
                            throw new Exception("Tags must be a JsonArray");
                        }
                        break;
                    case "summary":
                        Summary = item.Value?.ToString();
                        break;
                    case "description":
                        Description = item.Value?.ToString();
                        break;
                    case "operationId":
                        if (item.Value != null) {
                            operationId = item.Value?.ToString();
                        }
                        else {
                            throw new Exception("OperationId must not be null");
                        }
                        break;
                    case "produces":
                        if (item.Value is JsonArray) {
                            List<string> l = new();
                            foreach (var p in item.Value.AsArray()) {
                                if (p != null) {
                                    l.Add(p.ToString());
                                }
                            }
                            Produces = l.ToArray();
                        }
                        else {
                            throw new Exception("Produces must be a JsonArray");
                        }
                        break;
                    case "consumes":
                        if (item.Value is JsonArray) {
                            List<string> l = new();
                            foreach (var p in item.Value.AsArray()) {
                                if (p != null) {
                                    l.Add(p.ToString());
                                }
                            }
                            Consumes = l.ToArray();
                        }
                        else {
                            throw new Exception("Consumes must be a JsonArray");
                        }
                        break;
                    case "parameters":
                        if (item.Value is JsonArray) {
                            List<SwaggerParameter> l = new();
                            foreach (var p in item.Value.AsArray()) {
                                if (p is JsonObject) {
                                    l.Add(new SwaggerParameter(p.AsObject()));
                                }
                            }
                            Parameters = l.ToArray();
                        }
                        else {
                            throw new Exception("Parameters must be a JsonArray");
                        }
                        break;
                    case "responses":
                        if(item.Value is JsonObject) {
                            List<SwaggerResponse> l = new();
                            foreach (var r in item.Value.AsObject()) {
                                if(r.Value is JsonObject @object)
                                l.Add(new SwaggerResponse(r.Key, @object));
                            }
                            Responses = l.ToArray();
                        }
                        else {
                            throw new Exception("Responses must be a JsonObject");
                        }
                        break;
                    case "security":
                        if (item.Value is JsonArray) {
                            foreach (var p in item.Value.AsArray()) {
                                if(p is JsonObject) {
                                    Security = new Dictionary<string, string[]>();
                                    foreach (var s in p.AsObject()) {
                                        if(s.Value is JsonArray) {
                                            List<string> l = new();
                                            foreach (var s2 in s.Value.AsArray()) {
                                                if (s2 != null) {
                                                    l.Add(s2.ToString());
                                                }
                                            }
                                            Security.Add(s.Key, l.ToArray());
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            throw new Exception("Security must be a JsonArray");
                        }
                        break;
                    case "x-purecloud-method-name":
                        PurecloudMethodName = item.Value?.ToString();
                        break;
                    case "x-inin-requires-permissions":
                        IninRequiresPermissions = item.Value?.ToString();
                        break;
                    case "deprecated":
                        Deprecated = item.Value?.GetValue<bool>() ?? false;
                        break;
                    default:
                        Console.WriteLine(item.Key);
                        break;

                }
            }
            if(operationId != null) {
                OperationId = operationId;
            }
            else {
                throw new Exception("Missing operationId");
            }
        }
        public string Path { get; set; }
        public string Method { get; set; }
        public string[]? Tags { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string OperationId { get; set; }
        public string[]? Produces { get; set; }
        public string[]? Consumes { get; set; }
        public string? PurecloudMethodName { get; set; }
        public string? IninRequiresPermissions { get; set; }
        public bool Deprecated { get; set; }
        public Dictionary<string,string[]>? Security { get; set; }
        public SwaggerParameter[]? Parameters { get; set; }
        public SwaggerResponse[]? Responses { get; set; }
    }
}
