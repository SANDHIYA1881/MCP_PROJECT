namespace MCPSQLiteServer.Models;

public sealed class SelectQueryResult
{
    public IReadOnlyList<string> Columns { get; set; } = Array.Empty<string>();
    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; set; } = Array.Empty<IReadOnlyDictionary<string, object?>>();
}
