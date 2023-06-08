using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerParameter : SwaggerProperty {
        public SwaggerParameter(JsonObject o) : base("", o) {
            string? in_ = null;
            if (string.IsNullOrEmpty(Name)){
                throw new Exception("Missing name");
            }
            foreach (var item in o.AsObject()) {
                switch (item.Key) {
                    case "in":
                        in_ = item.Value?.ToString();
                        break;
                    case "required":
                        if(item.Value != null) {
                            Required = item.Value.GetValue<bool>();
                        }
                        break;
                    case "collectionFormat":
                        CollectionFormat = item.Value?.ToString();
                        break;
                    case "schema":
                        if (item.Value is JsonObject) {
                            var s = new SwaggerBase((JsonObject)item.Value);
                            foreach (var s1 in item.Value.AsObject()) {
                                switch (s1.Key) {
                                    // Handled in SwaggerBase
                                    case "$ref":
                                    case "type":
                                    case "items":
                                    case "additionalProperties":
                                        break;
                                    default:
                                        throw new Exception($"Invalid key {s1.Key} in schema");
                                }
                            }
                            Ref = s.Ref;
                            Type = s.Type;
                            Items = s.Items;
                            AdditionalProperties = s.AdditionalProperties;
                            EnumValues = s.EnumValues;
                        }
                        else {
                            throw new Exception("Schema must be a JsonObject");
                        }
                        break;
                    case "x-example":
                        Example = item.Value?.ToString();
                        break;
                    // Handled in SwaggerProperty
                    case "name":
                    case "description":
                    case "type":
                    case "format":
                    case "enum":
                    case "items":
                    case "default":
                        break;
                    default:
                        break;
                }
            }
            if(in_ == null) {
                throw new Exception("Missing in in parameter");
            }
            In = in_;
        }
        public bool Required { get; set; }
        // TODO: Make enum (csv, ssv, tsv, pipes, multi)
        public string? CollectionFormat { get; set; }
        public string In { get; set; }
    }
}
