using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerAdditionalProperties : SwaggerBase {
        public SwaggerAdditionalProperties(JsonObject o) : base(o) {
            foreach (var item in o) {
                switch(item.Key) {
                    case "uniqueItems":
                        UniqueItems = item.Value?.GetValue<bool>();
                        break;
                    // Handled in SwaggerBase
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
        }
        public bool? UniqueItems { get; private set; }
    }
}
