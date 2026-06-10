# MCPSQLiteServer

A .NET 8 console app skeleton that connects to Claude AI via the Anthropic API and stores conversation history in SQLite.
It can also run as an MCP server exposing read-only SQLite tools.

## Project structure

- `MCPSQLiteServer.csproj` - .NET 8 project file
- `Program.cs` - console app entry point
- `Database/SqliteService.cs` - SQLite persistence service
- `Services/ClaudeService.cs` - Claude AI HTTP client
- `Tools/AppConfig.cs` - loads environment configuration
- `Tools/SqliteMcpTools.cs` - MCP tools for SQLite inspection and read-only SELECT queries
- `Models/ConversationRecord.cs` - conversation record model
- `Models/ClaudeCompletionRequest.cs` / `ClaudeCompletionResponse.cs` - Claude request/response models

## Setup steps

1. Install .NET 8 SDK if it is not already installed.
2. Open `MCPSQLiteServer` in VS Code.
3. Set your Anthropic API key in the environment:
   - Windows PowerShell:
     ```powershell
     $env:ANTHROPIC_API_KEY = "your_api_key_here"
     ```
   - Or set it permanently in Windows environment variables.
4. Open a terminal in VS Code and run:
   ```powershell
   dotnet restore
   dotnet run
   ```

## MCP SQLite tools

The MCP server is available through stdio and is enabled with the `--mcp` argument:

```powershell
dotnet run -- --mcp
```

Expose this command to your MCP client with:

```json
{
  "mcpServers": {
    "sqlite": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Users\\ADMIN\\OneDrive\\Pictures\\MCPproject\\MCPSQLiteServer.csproj",
        "--",
        "--mcp"
      ]
    }
  }
}
```

Available MCP tools:

- `list_tables` - returns all user table names in `Database/chatdata.db`
- `get_schema` - returns columns, data types, and constraints for a table
- `run_select` - executes one read-only `SELECT` statement and returns rows

`run_select` opens SQLite in read-only mode and rejects non-`SELECT` statements, multiple statements, and write-operation keywords such as `INSERT`, `UPDATE`, `DELETE`, `DROP`, and `ALTER`.

## Notes

- The app uses the Claude completion endpoint at `https://api.anthropic.com/v1/complete`.
- The SQLite database file is written to `Database/chatdata.db`.
- If you want to use a different Claude model, set `ANTHROPIC_MODEL`.

## If dotnet is not found

The workspace environment currently does not have `dotnet` installed. Install the .NET 8 SDK from the official Microsoft site and verify it with:
```powershell
dotnet --version
```
