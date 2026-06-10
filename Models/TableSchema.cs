namespace MCPSQLiteServer.Models;

public sealed class TableSchema
{
    public string TableName { get; set; } = string.Empty;
    public IReadOnlyList<TableColumnSchema> Columns { get; set; } = Array.Empty<TableColumnSchema>();
    public string CreateSql { get; set; } = string.Empty;
}
