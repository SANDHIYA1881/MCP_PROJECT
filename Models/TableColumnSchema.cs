namespace MCPSQLiteServer.Models;

public sealed class TableColumnSchema
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool NotNull { get; set; }
    public string? DefaultValue { get; set; }
    public int PrimaryKeyPosition { get; set; }
}
