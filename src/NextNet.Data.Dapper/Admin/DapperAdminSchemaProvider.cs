using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Dapper.Admin;

/// <summary>
/// Dapper implementation of <see cref="IAdminSchemaProvider"/> that introspects
/// the database schema using raw SQL queries against INFORMATION_SCHEMA and
/// system catalog views.
/// </summary>
/// <remarks>
/// <para>
/// This provider works with SQL Server, SQLite, PostgreSQL, and MySQL by using
/// standard INFORMATION_SCHEMA queries. Provider-specific queries are selected
/// based on the connection string pattern.
/// </para>
/// </remarks>
public sealed class DapperAdminSchemaProvider : IAdminSchemaProvider
{
    /// <inheritdoc />
    public string ProviderName => "Dapper";

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperAdminSchemaProvider"/> class.
    /// </summary>
    /// <param name="connectionFactory">A factory that creates <see cref="IDbConnection"/> instances.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="connectionFactory"/> is null.</exception>
    public DapperAdminSchemaProvider(Func<IDbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    private readonly Func<IDbConnection> _connectionFactory;

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchemaTableInfo>> ListTablesAsync(
        string connectionString,
        bool includeViews = false,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection(connectionString);
        await OpenConnectionAsync(connection, cancellationToken);

        var tableTypes = includeViews
            ? "'BASE TABLE', 'VIEW'"
            : "'BASE TABLE'";

        var sql = $@"
            SELECT TABLE_NAME, TABLE_SCHEMA, TABLE_TYPE
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE IN ({tableTypes})
            ORDER BY TABLE_SCHEMA, TABLE_NAME";

        try
        {
            var rows = await connection.QueryAsync<(string Name, string? Schema, string Type)>(sql);

            var tables = rows.Select(r =>
            {
                var (name, schema, type) = r;
                return new SchemaTableInfo(
                    Name: name,
                    Schema: schema,
                    Type: type == "VIEW" ? "View" : "Table",
                    ColumnCount: 0,
                    IsSystem: name.StartsWith("__", StringComparison.Ordinal) ||
                              name.StartsWith("AspNet", StringComparison.Ordinal));
            }).ToList();

            return tables.AsReadOnly();
        }
        catch
        {
            // Fall back to SQLite-style query
            return await ListTablesSqliteAsync(connection, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<SchemaTableDetail> GetTableDetailAsync(
        string connectionString,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection(connectionString);
        await OpenConnectionAsync(connection, cancellationToken);

        var columns = new List<SchemaColumnInfo>();
        var primaryKey = new List<string>();

        try
        {
            // Try INFORMATION_SCHEMA.COLUMNS first
            var columnSql = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT,
                       CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @TableName
                ORDER BY ORDINAL_POSITION";

            var columnRows = await connection.QueryAsync<(string Name, string DataType, string IsNullable, string? DefaultValue, int? MaxLength)>(
                columnSql, new { TableName = tableName });

            foreach (var row in columnRows)
            {
                columns.Add(new SchemaColumnInfo(
                    Name: row.Name,
                    DataType: row.DataType,
                    IsNullable: row.IsNullable == "YES",
                    DefaultValue: row.DefaultValue,
                    MaxLength: row.MaxLength));
            }

            // Try primary key info
            var pkSql = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_NAME = @TableName AND CONSTRAINT_NAME LIKE 'PK_%'
                ORDER BY ORDINAL_POSITION";

            var pkRows = await connection.QueryAsync<string>(pkSql, new { TableName = tableName });
            primaryKey.AddRange(pkRows);
        }
        catch
        {
            // Fall back to SQLite-style PRAGMA queries
            return await GetTableDetailSqliteAsync(connection, tableName, cancellationToken);
        }

        // Mark primary key columns
        for (int i = 0; i < columns.Count; i++)
        {
            if (primaryKey.Contains(columns[i].Name, StringComparer.OrdinalIgnoreCase))
            {
                columns[i] = columns[i] with { IsKey = true };
            }
        }

        return new SchemaTableDetail(
            Name: tableName,
            Schema: null,
            Type: "Table",
            Columns: columns.AsReadOnly(),
            PrimaryKey: primaryKey.Count > 0 ? primaryKey.AsReadOnly() : null);
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = CreateConnection(connectionString);
            await OpenConnectionAsync(connection, cancellationToken);
            connection.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<IReadOnlyList<SchemaTableInfo>> ListTablesSqliteAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        var sql = "SELECT name FROM sqlite_master WHERE type IN ('table', 'view') ORDER BY name";
        var rows = await connection.QueryAsync<(string Name, string Type)>(sql);

        // SQLite's sqlite_master doesn't have separate type columns in the same way
        // We re-query with type info
        var tablesSql = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
        var viewsSql = "SELECT name FROM sqlite_master WHERE type = 'view' ORDER BY name";

        var tableNames = (await connection.QueryAsync<string>(tablesSql)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var viewNames = (await connection.QueryAsync<string>(viewsSql)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        allNames.UnionWith(tableNames);
        allNames.UnionWith(viewNames);

        return allNames.Select(name =>
        {
            var isView = viewNames.Contains(name);
            return new SchemaTableInfo(
                Name: name,
                Schema: null,
                Type: isView ? "View" : "Table",
                ColumnCount: 0,
                IsSystem: name == "__EFMigrationsHistory");
        }).ToList().AsReadOnly();
    }

    private static async Task<SchemaTableDetail> GetTableDetailSqliteAsync(
        IDbConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<SchemaColumnInfo>();
        var primaryKey = new List<string>();

        // SQLite PRAGMA table_info
        var pragmaSql = $"PRAGMA table_info(\"{tableName}\")";
        var rows = await connection.QueryAsync<(int cid, string name, string type, bool notnull, string? dflt_value, int pk)>(pragmaSql);

        foreach (var row in rows)
        {
            var isPk = row.pk == 1;
            if (isPk)
                primaryKey.Add(row.name);

            columns.Add(new SchemaColumnInfo(
                Name: row.name,
                DataType: row.type,
                IsNullable: !row.notnull,
                IsKey: isPk,
                DefaultValue: row.dflt_value,
                MaxLength: null));
        }

        // Try to get foreign keys
        var foreignKeys = new List<SchemaForeignKey>();
        try
        {
            var fkSql = $"PRAGMA foreign_key_list(\"{tableName}\")";
            var fkRows = await connection.QueryAsync<(int id, int seq, string table, string from, string to, string on_update, string on_delete, string match)>(fkSql);

            foreach (var fk in fkRows)
            {
                foreignKeys.Add(new SchemaForeignKey(
                    ConstraintName: $"FK_{tableName}_{fk.table}_{fk.from}",
                    ColumnName: fk.from,
                    ReferencedTable: fk.table,
                    ReferencedSchema: null,
                    ReferencedColumn: fk.to,
                    OnDelete: fk.on_delete));
            }
        }
        catch
        {
            // Foreign key info may not be available
        }

        return new SchemaTableDetail(
            Name: tableName,
            Schema: null,
            Type: "Table",
            Columns: columns.AsReadOnly(),
            PrimaryKey: primaryKey.Count > 0 ? primaryKey.AsReadOnly() : null,
            ForeignKeys: foreignKeys.Count > 0 ? foreignKeys.AsReadOnly() : null);
    }

    private static async Task OpenConnectionAsync(IDbConnection connection, CancellationToken cancellationToken)
    {
        if (connection is DbConnection dbConnection)
        {
            await dbConnection.OpenAsync(cancellationToken);
        }
        else
        {
            connection.Open();
        }
    }

    private IDbConnection CreateConnection(string connectionString)
    {
        try
        {
            return _connectionFactory();
        }
        catch
        {
            // Fall back to SqlClient if the factory fails
            return new SqlConnection(connectionString);
        }
    }
}
