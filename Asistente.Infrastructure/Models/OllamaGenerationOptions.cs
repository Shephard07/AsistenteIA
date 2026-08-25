using System.Text.Json.Serialization;

namespace Asistente.Infrastructure.Models;

public class OllamaGenerationOptions
{
    [JsonPropertyName("temperature")]
    public decimal Temperature { get; set; }

    [JsonPropertyName("num_predict")]
    public int NumPredict { get; set; }
}