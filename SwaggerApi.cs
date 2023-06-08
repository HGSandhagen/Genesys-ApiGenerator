using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiGenerator {
    internal class SwaggerApi {
        private List<SwaggerOperation> _operations = new List<SwaggerOperation>();
        public SwaggerApi(string path, JsonObject o) {
            Path = path;
            foreach (var item in o) {
                switch (item.Key) {
                    case "get":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Get", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "post":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Post", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "put":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Put", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "patch":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Patch", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "delete":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Delete", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    case "head":
                        if (item.Value is JsonObject) {
                            var op = new SwaggerOperation(path, "Head", (JsonObject)item.Value);
                            _operations.Add(op);
                        }
                        else {
                            throw new Exception("Operation must be a JsonObject");
                        }
                        break;
                    default:
                        Console.WriteLine(item.Key);
                        break;
                }
            }
        }
        public string Path { get; set; }
        public SwaggerOperation[] Operations { get => _operations.ToArray(); }
    }
}
