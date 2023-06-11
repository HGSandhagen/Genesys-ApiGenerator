using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class NotificationSwaggerDefinition {
        public NotificationSwaggerDefinition(string id, string? type, string? description) {
            if (!string.IsNullOrEmpty(id)) {
                Id = id;
                string[] n = id.Replace("urn:jsonschema:", "").Split(':');
                Name = string.Join("", n.Select(p => p.Substring(0, 1).ToUpper() + p.Substring(1)));
            }
            else {
                throw new Exception("No id found for notification.");
            }
            Type = type;
            Description = description;
            Properties = new NotificationProperty[0];
            IsAlias = true;
        }
        public NotificationSwaggerDefinition(string id, string? type, NotificationProperty[] properties, string[]? required, string? description) {
            if (!string.IsNullOrEmpty(id)) {
                Id = id;
                string[] n = id.Replace("urn:jsonschema:", "").Split(':');
                Name = string.Join("", n.Select(p => p.Substring(0, 1).ToUpper() + p.Substring(1)));
            }
            else {
                throw new Exception("No id found for notification.");
            }
            Type = type;
            Properties = properties;
            Required = required; 
            Description = description;
        }
        //public NotificationSwaggerDefinition(JsonObject o) {
        //    string? id = null;
        //    foreach (var item in o) {
        //        switch (item.Key) {
        //            case "id":
        //                 id = item.Value?.ToString();
        //                break;
        //            case "type":
        //                Type = item.Value?.ToString();
        //                break;
        //            case "properties":
        //                if (item.Value != null) {
        //                    foreach (var prop in item.Value.AsObject()) {
        //                        if (prop.Value != null) {
        //                            Properties.Add(new NotificationProperty(prop.Key, prop.Value.AsObject()));
        //                        }
        //                    }
        //                }
        //                break;
        //            case "required":
        //                if (item.Value is JsonArray) {
        //                    var list = new List<string>();
        //                    foreach (var prop in item.Value.AsArray()) {
        //                        if (prop != null) {
        //                            list.Add(prop.ToString());
        //                        }
        //                    }
        //                    Required = list.ToArray();
        //                }
        //                break;
        //            case "description":
        //                Description = item.Value?.ToString();
        //                break;
        //            default:
        //                Console.WriteLine(item.Key);
        //                break;
        //        }
        //    }
        //    if(!string.IsNullOrEmpty(id)) {
        //        Id = id;
        //        string[] n = id.Replace("urn:jsonschema:","").Split(':');
        //        Name = string.Join("", n.Select(p => p.Substring(0, 1).ToUpper() + p.Substring(1)));
        //    }
        //    else {
        //        throw new Exception("No id found for notification.");
        //    }
        //}
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string? Description { get; set; }
        public string? Type { get; set; }
        public NotificationProperty[] Properties { get; set; }
        public string[]? Required { get; set; }
        public bool IsAlias { get; set; }
        public string[]? TopicParameters { get; set; }
    }
}
