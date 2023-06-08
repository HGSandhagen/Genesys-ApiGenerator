using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class SwaggerBase {
        public SwaggerBase(JsonObject o) {
            foreach (var item in o) {
                switch (item.Key) {
                    case "$ref":
                        Ref = item.Value?.ToString();
                        break;
                    case "type":
                        Type = item.Value?.ToString();
                        break;
                    case "enum":
                        if (item.Value is JsonArray) {
                            var list = new List<string>();
                            foreach (var prop in item.Value.AsArray()) {
                                if (prop != null) {
                                    list.Add(prop.ToString());
                                }
                            }
                            EnumValues = list.ToArray();
                        }
                        break;
                    case "format":
                        Format = item.Value?.ToString();
                        break;
                    case "items":
                        if (item.Value is JsonObject)
                            Items = new SwaggerArray((JsonObject)item.Value);
                        break;
                    case "additionalProperties":
                        if (item.Value is JsonObject) {
                            AdditionalProperties = new SwaggerAdditionalProperties((JsonObject)item.Value);
                        }
                        break;
                    case "default":
                        Default = item.Value?.ToString();
                        break;
                }
            }
            if(Type == "array" && Items == null) {
                throw new Exception("Array must have Items");
            }
        }
        public string? Ref { get; protected set; }
        public string? Type { get; protected set; }
        public string? Format { get; private set; }
        public SwaggerArray? Items { get; protected set; }
        public string[]? EnumValues { get; protected set; }
        public SwaggerAdditionalProperties? AdditionalProperties { get; protected set; }
        public string? Default {  get; private set; }
        public TypeInfo GetTypeInfo(string propName, bool isCollection = false) {
            if (Ref != null) {
                return new TypeInfo(Ref.Replace("#/definitions/", ""), isCollection);
            }
            if (Type == null) {
                throw new Exception("Type is null");
            }
            switch (Type) {
                case "array":
                    if (Items != null) {
                        //typeName = GetArrayType(propName, Items);
                        return Items.GetTypeInfo(propName, isCollection = true);
                    }
                    else {
                        throw new Exception("Items missing for array");
                    }
                case "string":
                    if (Format == "date-time") {
                        return new TypeInfo("DateTimeOffset", isCollection);
                    }
                    else if (EnumValues != null) {
                        return new TypeInfo(ApiGenerator.CreateName(propName) + "Constant", isCollection, enumModel: new EnumModel(ApiGenerator.CreateName(propName) + "Constant", EnumValues));
                    }
                    else if (Format == "uri") {
                        return new TypeInfo("Uri", isCollection);
                    }
                    else if (Format == "date") {
                        return new TypeInfo("DateOnly", isCollection);
                    }
                    else if (Format == "local-date-time") {
                        return new TypeInfo("DateTime", isCollection);
                    }
                    else if (Format == "url") {
                        return new TypeInfo("string", isCollection);
                    }
                    else if (Format == "interval") {
                        return new TypeInfo("DateTimeInterval", isCollection);
                    }
                    else if (Format != null) {
                        throw new Exception("Unknown format " + Format);
                    }
                    else {
                        return new TypeInfo("string", isCollection);
                    }
                case "boolean":
                    return new TypeInfo("bool", isCollection);
                case "integer":
                    if (Format == "int64") {
                        return new TypeInfo("long", isCollection);
                    }
                    else {
                        return new TypeInfo("int", isCollection);
                    }
                case "number":
                    if (Format == "float") {
                        return new TypeInfo("float", isCollection);
                    }
                    else {
                        return new TypeInfo("double", isCollection);
                    }
                case "object":
                    if (AdditionalProperties != null) {
                        if (AdditionalProperties.Ref != null) {
                            return new TypeInfo(AdditionalProperties.Ref.Replace("#/definitions/", ""), isCollection, isDictionary: true);
                        }
                        else if (AdditionalProperties.Type == "array") {
                            if (AdditionalProperties.Items != null) {
                                return AdditionalProperties.Items.GetTypeInfo(propName, isCollection = true);
                            }
                            else {
                                throw new Exception("array missing items");
                            }
                        }
                        else if (AdditionalProperties.Type == "string") {
                            if (AdditionalProperties.EnumValues != null) {
                                // TODO: Check enum
                                return new TypeInfo("string", isCollection, isDictionary: true);
                            }
                            if (AdditionalProperties.Format != null) {
                                throw new Exception($"Unknown string format {AdditionalProperties.Format} for additional property");
                            }
                            else {
                                return new TypeInfo("string", isCollection, isDictionary: true);
                            }
                        }
                        else if (AdditionalProperties.Type == "integer") {
                            if (AdditionalProperties.Format == "int64") {
                                return new TypeInfo("long", isCollection, isDictionary: true);
                            }
                            else {
                                return new TypeInfo("int", isCollection, isDictionary: true);
                            }
                        }
                        else if (AdditionalProperties.Type == "number") {
                            if (Format == "float") {
                                return new TypeInfo("float", isCollection, isDictionary: true);
                            }
                            else {
                                return new TypeInfo("double", isCollection, isDictionary: true);
                            }
                        }
                        else if (AdditionalProperties.Type == "object") {
                            return new TypeInfo("object", isCollection, isDictionary: true);
                        }
                        else if (AdditionalProperties.Type == "boolean") {
                            return new TypeInfo("bool", isCollection, isDictionary: true);
                        }
                        else {
                            throw new Exception($"Unknown type {AdditionalProperties.Type} in additional properties");
                        }
                    }
                    else {
                        return new TypeInfo("object", isCollection);
                    }
                default:
                    throw new Exception("Unknown type" + Type);
            }

        }
    }
}
