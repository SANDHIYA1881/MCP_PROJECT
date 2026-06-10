using System.Text.Json.Serialization;

namespace MCPSQLiteServer.Models;

public sealed class ClaudeCompletionResponse
{
    [JsonPropertyName("completion")]
    public string Completion { get; set; } = string.Empty;
}
