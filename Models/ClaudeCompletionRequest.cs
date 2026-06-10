using System.Text.Json.Serialization;

namespace MCPSQLiteServer.Models;

public sealed class ClaudeCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("max_tokens_to_sample")]
    public int MaxTokensToSample { get; set; }

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; }

    [JsonPropertyName("stop_sequences")]
    public string[] StopSequences { get; set; } = Array.Empty<string>();
}
