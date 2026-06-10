using MCPSQLiteServer.Database;
using MCPSQLiteServer.Services;
using MCPSQLiteServer.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

var config = AppConfig.Load();

var dbPath = Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory,
    "Database",
    "chatdata.db");

var sqlite = new SqliteService(dbPath);
sqlite.Initialize();

if (args.Contains("--mcp", StringComparer.OrdinalIgnoreCase))
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Logging.AddConsole(options =>
    {
        options.LogToStandardErrorThreshold = LogLevel.Trace;
    });

    builder.Services.AddSingleton<SqliteService>(sqlite);
builder.Services.AddSingleton<SqliteMcpTools>();

    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithToolsFromAssembly();

    await builder.Build().RunAsync();
    return;
}

var claude = new ClaudeService(
    config.AnthropicApiKey,
    config.AnthropicModel);

Console.WriteLine("MCPSQLiteServer: Claude AI + SQLite sample");
Console.WriteLine("Make sure ANTHROPIC_API_KEY is set in your environment.");
Console.WriteLine();

var prompt =
    "Create a helpful greeting explaining how this .NET console app stores a conversation with Claude AI in SQLite.";

var assistantText = await claude.GetCompletionAsync(prompt);

sqlite.SaveMessage("user", prompt);
sqlite.SaveMessage("assistant", assistantText);

Console.WriteLine("Claude response:");
Console.WriteLine(assistantText);
Console.WriteLine();
Console.WriteLine($"Saved conversation records to database: {dbPath}");