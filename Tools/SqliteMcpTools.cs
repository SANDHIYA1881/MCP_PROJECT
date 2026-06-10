using System.ComponentModel;
using MCPSQLiteServer.Database;
using MCPSQLiteServer.Models;
using ModelContextProtocol.Server;

namespace MCPSQLiteServer.Tools;

[McpServerToolType]
public sealed class SqliteMcpTools
{
    private readonly SqliteService _sqlite;

    public SqliteMcpTools(SqliteService sqlite)
    {
        _sqlite = sqlite;
    }

    [McpServerTool(Name = "list_tables", ReadOnly = true, Destructive = false)]
    [Description("Returns all table names available in the SQLite database.")]
    public IReadOnlyList<string> ListTables()
    {
        return _sqlite.ListTables();
    }

    [McpServerTool(Name = "get_schema", ReadOnly = true, Destructive = false)]
    [Description("Returns the schema, including columns, data types, and constraints, for a specified SQLite table.")]
    public TableSchema GetSchema(
        [Description("The table name to inspect.")] string tableName)
    {
        return _sqlite.GetSchema(tableName);
    }

    [McpServerTool(Name = "run_select", ReadOnly = true, Destructive = false)]
    [Description("Executes a single read-only SELECT query against the SQLite database and returns the results.")]
    public SelectQueryResult RunSelect(
        [Description("A single SELECT statement. Write operations are blocked.")] string query)
    {
        return _sqlite.RunSelect(query);
    }
}
