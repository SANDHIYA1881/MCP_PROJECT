using Microsoft.Data.Sqlite;
using MCPSQLiteServer.Models;
using System.Text;

namespace MCPSQLiteServer.Database;

public sealed class SqliteService
{
    private readonly string _databaseFile;

    public SqliteService(string databaseFile)
    {
        _databaseFile = databaseFile;
    }

    public void Initialize()
    {
        var directory = Path.GetDirectoryName(_databaseFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var connection = new SqliteConnection($"Data Source={_databaseFile}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Conversations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Role TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public void SaveMessage(string role, string content)
    {
        using var connection = new SqliteConnection($"Data Source={_databaseFile}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Conversations (Role, Content, CreatedAt)
            VALUES ($role, $content, $createdAt);";
        command.Parameters.AddWithValue("$role", role);
        command.Parameters.AddWithValue("$content", content);
        command.Parameters.AddWithValue("$createdAt", DateTime.UtcNow.ToString("o"));
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<string> ListTables()
    {
        using var connection = new SqliteConnection($"Data Source={_databaseFile}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT name
            FROM sqlite_schema
            WHERE type = 'table'
              AND name NOT LIKE 'sqlite_%'
            ORDER BY name;";

        var tables = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }

    public TableSchema GetSchema(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException("Table name is required.", nameof(tableName));
        }

        using var connection = new SqliteConnection($"Data Source={_databaseFile}");
        connection.Open();

        if (!TableExists(connection, tableName))
        {
            throw new InvalidOperationException($"Table '{tableName}' does not exist.");
        }

        var columns = new List<TableColumnSchema>();
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT name, type, [notnull], dflt_value, pk
                FROM pragma_table_info($tableName)
                ORDER BY cid;";
            command.Parameters.AddWithValue("$tableName", tableName);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                columns.Add(new TableColumnSchema
                {
                    Name = reader.GetString(0),
                    DataType = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    NotNull = reader.GetInt32(2) == 1,
                    DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                    PrimaryKeyPosition = reader.GetInt32(4)
                });
            }
        }

        string createSql;
        using (var command = connection.CreateCommand())
        {
            command.CommandText = @"
                SELECT sql
                FROM sqlite_schema
                WHERE type = 'table'
                  AND name = $tableName;";
            command.Parameters.AddWithValue("$tableName", tableName);
            createSql = Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
        }

        return new TableSchema
        {
            TableName = tableName,
            Columns = columns,
            CreateSql = createSql
        };
    }

    public SelectQueryResult RunSelect(string query)
    {
        if (!IsSelectOnly(query))
        {
            throw new InvalidOperationException("Only a single read-only SELECT statement is allowed.");
        }

        using var connection = new SqliteConnection($"Data Source={_databaseFile};Mode=ReadOnly");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandTimeout = 30;

        using var reader = command.ExecuteReader();
        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(reader.GetName)
            .ToArray();
        var rows = new List<IReadOnlyDictionary<string, object?>>();

        while (reader.Read())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (var column in columns)
            {
                var ordinal = reader.GetOrdinal(column);
                row[column] = reader.IsDBNull(ordinal) ? null : reader.GetValue(ordinal);
            }

            rows.Add(row);
        }

        return new SelectQueryResult
        {
            Columns = columns,
            Rows = rows
        };
    }

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM sqlite_schema
            WHERE type = 'table'
              AND name = $tableName
              AND name NOT LIKE 'sqlite_%';";
        command.Parameters.AddWithValue("$tableName", tableName);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private static bool IsSelectOnly(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return false;
        }

        var sanitized = StripCommentsAndLiterals(query).Trim();
        if (sanitized.Length == 0)
        {
            return false;
        }

        var statements = sanitized
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (statements.Length != 1)
        {
            return false;
        }

        var statement = statements[0].TrimStart();
        if (!statement.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var blockedKeywords = new[]
        {
            "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", "REPLACE",
            "TRUNCATE", "UPSERT", "VACUUM", "ATTACH", "DETACH", "REINDEX",
            "ANALYZE", "PRAGMA", "BEGIN", "COMMIT", "ROLLBACK", "SAVEPOINT"
        };

        var paddedStatement = $" {statement} ";
        return !blockedKeywords.Any(keyword =>
            paddedStatement.Contains($" {keyword} ", StringComparison.OrdinalIgnoreCase));
    }

    private static string StripCommentsAndLiterals(string query)
    {
        var builder = new StringBuilder(query.Length);
        var index = 0;

        while (index < query.Length)
        {
            var current = query[index];
            var next = index + 1 < query.Length ? query[index + 1] : '\0';

            if (current == '-' && next == '-')
            {
                index += 2;
                while (index < query.Length && query[index] != '\n')
                {
                    index++;
                }
                builder.Append(' ');
                continue;
            }

            if (current == '/' && next == '*')
            {
                index += 2;
                while (index + 1 < query.Length && !(query[index] == '*' && query[index + 1] == '/'))
                {
                    index++;
                }
                index = Math.Min(index + 2, query.Length);
                builder.Append(' ');
                continue;
            }

            if (current is '\'' or '"')
            {
                var quote = current;
                index++;
                while (index < query.Length)
                {
                    if (query[index] == quote)
                    {
                        if (index + 1 < query.Length && query[index + 1] == quote)
                        {
                            index += 2;
                            continue;
                        }

                        index++;
                        break;
                    }

                    index++;
                }
                builder.Append(" '' ");
                continue;
            }

            builder.Append(current);
            index++;
        }

        return builder.ToString();
    }
}
