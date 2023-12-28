using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerResponse {
        public SwaggerResponse(string name, JsonObject o) { 
            this.Name = name;
            foreach (var item in o) {
                switch(item.Key) {
                    case "description":
                        Description = item.Value?.ToString();
                        break;
                    case "schema":
                        if(item.Value is JsonObject) {
                            Content = new SwaggerBase((JsonObject)item.Value);
                        }
                        break;
                    case "headers":
                        break;
                    case "x-inin-error-codes":
                        if(item.Value is JsonObject) {
                            IninErrorCodes = new();
                            foreach (var e in item.Value.AsObject()) {
                                IninErrorCodes.Add(e.Key, e.Value?.ToString() ?? "");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
        public string Name { get; set; }
        public string? Description { get; set; }
        public SwaggerBase? Content { get; set; }
        public Dictionary<string,string>? IninErrorCodes { get; set; }

    }
}
