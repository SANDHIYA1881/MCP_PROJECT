using System.Net.Http.Headers;
using System.Net.Http.Json;
using MCPSQLiteServer.Models;

namespace MCPSQLiteServer.Services;

public sealed class ClaudeService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public ClaudeService(string apiKey, string model)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("ANTHROPIC_API_KEY is required.", nameof(apiKey));
        }

        _model = string.IsNullOrWhiteSpace(model) ? "claude-3.5" : model;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.anthropic.com")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> GetCompletionAsync(string prompt)
    {
        var request = new ClaudeCompletionRequest
        {
            Model = _model,
            Prompt = BuildPrompt(prompt),
            MaxTokensToSample = 300,
            Temperature = 0.2,
            StopSequences = new[] { "\n\nHuman:" }
        };

        var response = await _httpClient.PostAsJsonAsync("/v1/complete", request);
        response.EnsureSuccessStatusCode();

        var completion = await response.Content.ReadFromJsonAsync<ClaudeCompletionResponse>();
        if (completion == null || string.IsNullOrWhiteSpace(completion.Completion))
        {
            throw new InvalidOperationException("Claude API returned an empty response.");
        }

        return completion.Completion.Trim();
    }

    private static string BuildPrompt(string userText)
    {
        return $"\n\nHuman: {userText}\n\nAssistant:";
    }
}
