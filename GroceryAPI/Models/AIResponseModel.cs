#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GroceryAPI.Models;

public sealed class UpstreamResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("object")] public string Object { get; set; } = "";
    // epoch seconds
    [JsonPropertyName("created_at")] public long CreatedAt { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("background")] public bool? Background { get; set; }
    [JsonPropertyName("error")] public JsonElement? Error { get; set; }
    [JsonPropertyName("model")] public string Model { get; set; } = "";
    [JsonPropertyName("output")] public List<UpstreamOutputItem> Output { get; set; } = new();

    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class UpstreamOutputItem
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("role")] public string? Role { get; set; }

    [JsonPropertyName("content")]
    public List<UpstreamContentItem> Content { get; set; } = new();

    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}

public sealed class UpstreamContentItem
{
    [JsonPropertyName("type")] public string Type { get; set; } = "";
    [JsonPropertyName("text")] public string? Text { get; set; }

    [JsonPropertyName("annotations")] public JsonElement? Annotations { get; set; }
    [JsonPropertyName("logprobs")] public JsonElement? Logprobs { get; set; }

    [JsonExtensionData] public Dictionary<string, JsonElement>? Extra { get; set; }
}