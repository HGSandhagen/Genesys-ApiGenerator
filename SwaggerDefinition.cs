using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerDefinition {
        public SwaggerDefinition(string name, JsonObject o) {
            Name = name;
            foreach (var item in o) {
                switch (item.Key) {
                    case "type":
                        Type = item.Value?.ToString();
                        break;
                    case "properties":
                        if (item.Value != null) {
                            foreach (var prop in item.Value.AsObject()) {
                                if (prop.Value != null) {
                                    Properties.Add(new SwaggerProperty(prop.Key, prop.Value.AsObject()));
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
                            Required = list.ToArray();
                        }
                        break;
                    case "description":
                        Description = item.Value?.ToString();
                        break;
                    default:
                        Console.WriteLine(item.Key);
                        break;
                }
            }

        }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public List<SwaggerProperty> Properties { get; set; } = new List<SwaggerProperty>();
        public string[]? Required { get; set; }
    }
}
