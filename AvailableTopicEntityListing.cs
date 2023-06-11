using System.Text.Json.Serialization;

public class AvailableTopicEntityListing {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [JsonPropertyName("entities")]
    public IEnumerable<AvailableTopic>? Entities { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

}
