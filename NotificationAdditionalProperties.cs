using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class NotificationAdditionalProperties : NotificationBase {
        public NotificationAdditionalProperties(bool? uniqueItems, NotificationProperty[]? properties, NotificationBase b) : base(b) { 
        UniqueItems = uniqueItems;
            Properties = properties;
        }
        //public NotificationAdditionalProperties(JsonObject o) : base(o) {
        //    foreach (var item in o) {
        //        switch (item.Key) {
        //            case "uniqueItems":
        //                UniqueItems = item.Value?.GetValue<bool>();
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
        //            case "format":
        //            case "items":
        //            case "enum":
        //                break;
        //            default:
        //                Console.WriteLine($"case \"{item.Key}\":\r\n\tbreak;");
        //                break;
        //        }
        //    }
        //}
        public bool? UniqueItems { get; private set; }
        public NotificationProperty[]? Properties { get; set; }
    }
}
