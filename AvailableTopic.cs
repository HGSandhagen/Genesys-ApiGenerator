using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ApiGenerator;

public class AvailableTopic {
    [JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]
    public enum VisibilityConstant {
        [JsonEnumName("Public")]
        Public,
        [JsonEnumName("Preview")]
        Preview,
    }
    [JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]
    public enum TransportsConstant {
        [JsonEnumName("All")]
        All,
        [JsonEnumName("Websocket")]
        Websocket,
        [JsonEnumName("EventBridge")]
        EventBridge,
        [JsonEnumName("ProcessAutomation")]
        ProcessAutomation,
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    //[JsonPropertyName("permissionDetails")]
    //public IEnumerable<PermissionDetails>? PermissionDetails { get; set; }
    [JsonPropertyName("requiresPermissions")]
    public IEnumerable<string>? RequiresPermissions { get; set; }
    [JsonPropertyName("requiresDivisionPermissions")]
    public bool? RequiresDivisionPermissions { get; set; }
    [JsonPropertyName("requiresAnyValidator")]
    public bool? RequiresAnyValidator { get; set; }
    [JsonPropertyName("enforced")]
    public bool? Enforced { get; set; }
    [JsonPropertyName("visibility")]
    public VisibilityConstant? Visibility { get; set; }
    [JsonPropertyName("schema")]
    public JsonObject? Schema { get; set; }
    [JsonPropertyName("requiresCurrentUser")]
    public bool? RequiresCurrentUser { get; set; }
    [JsonPropertyName("requiresCurrentUserOrPermission")]
    public bool? RequiresCurrentUserOrPermission { get; set; }
    [JsonPropertyName("transports")]
    public IEnumerable<TransportsConstant>? Transports { get; set; }
    [JsonPropertyName("publicApiTemplateUriPaths")]
    public IEnumerable<string>? PublicApiTemplateUriPaths { get; set; }
    [JsonPropertyName("topicParameters")]
    public IEnumerable<string>? TopicParameters { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
