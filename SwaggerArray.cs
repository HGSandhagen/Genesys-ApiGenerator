using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerArray : SwaggerBase {
        public SwaggerArray(JsonObject o) : base(o) {
            foreach (var item in o) {
                switch (item.Key) {
                    case "description":
                        Description = item.Value?.ToString();
                        break;
                    // Handled in SwaggerBase
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
        }
        public string? Description { get; set; }
    
    }
}
