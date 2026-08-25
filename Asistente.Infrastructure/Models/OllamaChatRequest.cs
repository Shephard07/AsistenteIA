using System.Text.Json.Serialization;

namespace Asistente.Infrastructure.Models;

public class OllamaChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<OllamaChatMessage> Messages { get; set; } = [];

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    [JsonPropertyName("think")]
    public bool Think { get; set; }

    [JsonPropertyName("keep_alive")]
    public string KeepAlive { get; set; } = "0";

    [JsonPropertyName("options")]
    public OllamaGenerationOptions? Options { get; set; }
}