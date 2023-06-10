using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class NotificationArray : NotificationBase {
        public NotificationArray(string? description, NotificationProperty[]? properties, NotificationBase b): base(b) {
            Description = description;
            Properties = properties;
        }
        //public NotificationArray(JsonObject o) : base(o) {
        //    foreach (var item in o) {
        //        switch (item.Key) {
        //            case "description":
        //                Description = item.Value?.ToString();
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
        //            // Handled in NotificationBase
        //            case "id":
        //            case "$ref":
        //            case "type":
        //            case "enum":
        //            case "format":
        //            case "additionalProperties":
        //            case "default":
        //                break;
        //            default:
        //                throw new Exception($"Unknown key {item.Key} in item array");
        //        }
        //    }
        //}
        public string? Description { get; set; }
        public NotificationProperty[]? Properties { get; set; }
    }
}
