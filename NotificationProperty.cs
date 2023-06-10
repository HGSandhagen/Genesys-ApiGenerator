using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class NotificationProperty : NotificationBase {
        public NotificationProperty(string name,string? description, NotificationProperty[]? properties, bool? readOnly,bool? uniqueItems,string? example,string? genesysEntityType,
            string[]? genesysSearchFields,double? minimum,double? maximum, int? minItems, int? maxItems, int? minLength,int? maxLength,int? position, NotificationBase @base)
            : base(@base) { 
            Name = name;
            Description = description;
            Properties = properties;
            ReadOnly = readOnly;
            UniqueItems = uniqueItems;
            Example = example;
            GenesysEntityType = genesysEntityType;
            GenesysSearchFields = genesysSearchFields;
            Minimum = minimum;
            Maximum = maximum;
            MinItems = minItems;
            MaxItems = maxItems;
            MinLength = minLength;
            MaxLength = maxLength;
            Position = position;
        }
        //public NotificationProperty(string name, JsonObject value) : base(value) {
        //    Name = name;
        //    ParseValue(value);
        //}
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public NotificationProperty[]? Properties { get; private set; }
        public bool? ReadOnly { get; private set; }
        public bool? UniqueItems { get; private set; }
        public string? Example { get; protected set; }
        public string? GenesysEntityType { get; private set; }
        public string[]? GenesysSearchFields { get; private set; }
        public double? Minimum { get; private set; }
        public double? Maximum { get; private set; }
        public int? MaxItems { get; private set; }
        public int? MinItems { get; private set; }
        public int? MinLength { get; private set; }
        public int? MaxLength { get; private set; }
        public int? Position { get; private set; }
        //void ParseValue(JsonObject value) {
        //    foreach (var item in value) {
        //        switch (item.Key) {
        //            case "name":
        //                if (item.Value != null) {
        //                    Name = item.Value.ToString();
        //                }
        //                else {
        //                    throw new Exception("Name must not be null");
        //                }
        //                break;
        //            case "description":
        //                Description = item.Value?.ToString();
        //                break;

        //            case "readOnly":
        //                ReadOnly = item.Value?.GetValue<bool>();
        //                break;
        //            case "uniqueItems":
        //                UniqueItems = item.Value?.GetValue<bool>();
        //                break;
        //            case "example":
        //                Example = item.Value?.ToString();
        //                break;
        //            case "x-genesys-entity-type":
        //                GenesysEntityType = item.Value?.ToString();
        //                break;
        //            case "x-genesys-search-fields":
        //                if (item.Value is JsonArray) {
        //                    var list = new List<string>();
        //                    foreach (var prop in item.Value.AsArray()) {
        //                        if (prop != null) {
        //                            list.Add(prop.ToString());
        //                        }
        //                    }
        //                    GenesysSearchFields = list.ToArray();
        //                }
        //                break;
        //            case "minimum":
        //                Minimum = item.Value?.GetValue<double>();
        //                break;
        //            case "maximum":
        //                Maximum = item.Value?.GetValue<double>();
        //                break;
        //            case "maxItems":
        //                MaxItems = item.Value?.GetValue<int>();
        //                break;
        //            case "minItems":
        //                MinItems = item.Value?.GetValue<int>();
        //                break;
        //            case "minLength":
        //                MinLength = item.Value?.GetValue<int>();
        //                break;
        //            case "maxLength":
        //                MaxLength = item.Value?.GetValue<int>();
        //                break;
        //            case "position":
        //                Position = item.Value?.GetValue<int>();
        //                break;
        //            // Handled in SwaggerBase
        //            case "$ref":
        //            case "type":
        //            case "format":
        //            case "enum":
        //            case "items":
        //            case "additionalProperties":
        //                break;
        //            default:
        //                break;
        //                //throw new Exception($"Unknown key {item.Key} in property");

        //        }
        //    }
        //}

    }
}
