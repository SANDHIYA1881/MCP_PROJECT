namespace MCPSQLiteServer.Tools;

public sealed class AppConfig
{
    public string AnthropicApiKey { get; init; } = string.Empty;
    public string AnthropicModel { get; init; } = string.Empty;

    public static AppConfig Load()
    {
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? string.Empty;
        var model = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-3.5";

        return new AppConfig
        {
            AnthropicApiKey = apiKey,
            AnthropicModel = model
        };
    }
}
