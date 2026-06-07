using Microsoft.Data.SqlClient;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.EntityFramework.Admin;

/// <summary>
/// EF Core implementation of <see cref="IAdminSchemaProvider"/> that introspects
/// the database schema using raw SQL queries against INFORMATION_SCHEMA.
/// </summary>
/// <remarks>
/// <para>
/// This provider works with any EF Core-compatible database and uses:
/// <list type="bullet">
///   <item><description>INFORMATION_SCHEMA queries for column-level and constraint details.</description></item>
///   <item><description>Provider-specific queries (SQLite PRAGMA) as a fallback.</description></item>
/// </list>
/// </para>
/// <para>
/// The provider does not require an application DbContext — it connects directly
/// using the configured connection string. This allows schema exploration without
/// a running application.
/// </para>
/// </remarks>
public sealed class EfCoreAdminSchemaProvider : IAdminSchemaProvider
{
    /// <inheritdoc />
    public string ProviderName => "EntityFramework";

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCoreAdminSchemaProvider"/> class.
    /// </summary>
    public EfCoreAdminSchemaProvider()
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchemaTableInfo>> ListTablesAsync(
        string connectionString,
        bool includeViews = false,
        CancellationToken cancellationToken = default)
    {
        var tables = new List<SchemaTableInfo>();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            var tableTypes = includeViews
                ? "'BASE TABLE', 'VIEW'"
                : "'BASE TABLE'";

            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT TABLE_NAME, TABLE_SCHEMA, TABLE_TYPE
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE IN ({tableTypes})
                ORDER BY TABLE_NAME";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var tableName = reader.GetString(0);
                var schema = reader.IsDBNull(1) ? null : reader.GetString(1);
                var tableType = reader.GetString(2);

                tables.Add(new SchemaTableInfo(
                    Name: tableName,
                    Schema: schema,
                    Type: tableType == "VIEW" ? "View" : "Table",
                    ColumnCount: 0,
                    IsSystem: tableName.StartsWith("__", StringComparison.Ordinal) ||
                              tableName.StartsWith("AspNet", StringComparison.Ordinal)));
            }
        }
        catch
        {
            // INFORMATION_SCHEMA not available; try SQLite fallback
            return await ListTablesSqliteAsync(connectionString, cancellationToken);
        }

        return tables.AsReadOnly();
    }

    /// <inheritdoc />
    public async Task<SchemaTableDetail> GetTableDetailAsync(
        string connectionString,
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var columns = new List<SchemaColumnInfo>();
        var primaryKey = new List<string>();
        var foreignKeys = new List<SchemaForeignKey>();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Query column information
            using var columnCommand = connection.CreateCommand();
            columnCommand.CommandText = @"
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT,
                       CHARACTER_MAXIMUM_LENGTH
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @tableName
                ORDER BY ORDINAL_POSITION";

            var param = columnCommand.CreateParameter();
            param.ParameterName = "@tableName";
            param.Value = tableName;
            columnCommand.Parameters.Add(param);

            using var columnReader = await columnCommand.ExecuteReaderAsync(cancellationToken);
            while (await columnReader.ReadAsync(cancellationToken))
            {
                var colName = columnReader.GetString(0);
                var dataType = columnReader.GetString(1);
                var isNullable = columnReader.GetString(2) == "YES";
                var defaultValue = columnReader.IsDBNull(3) ? null : columnReader.GetString(3);
                var maxLength = columnReader.IsDBNull(4) ? null : (int?)columnReader.GetInt32(4);

                columns.Add(new SchemaColumnInfo(
                    Name: colName,
                    DataType: dataType,
                    IsNullable: isNullable,
                    DefaultValue: defaultValue,
                    MaxLength: maxLength));
            }

            // Primary key info
            try
            {
                using var pkCommand = connection.CreateCommand();
                pkCommand.CommandText = @"
                    SELECT COLUMN_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                    WHERE TABLE_NAME = @tableName AND CONSTRAINT_NAME LIKE 'PK_%'
                    ORDER BY ORDINAL_POSITION";

                var pkParam = pkCommand.CreateParameter();
                pkParam.ParameterName = "@tableName";
                pkParam.Value = tableName;
                pkCommand.Parameters.Add(pkParam);

                using var pkReader = await pkCommand.ExecuteReaderAsync(cancellationToken);
                while (await pkReader.ReadAsync(cancellationToken))
                {
                    primaryKey.Add(pkReader.GetString(0));
                }
            }
            catch
            {
                // PK info may not be available
            }

            // Foreign keys
            try
            {
                using var fkCommand = connection.CreateCommand();
                fkCommand.CommandText = @"
                    SELECT
                        KCU.CONSTRAINT_NAME,
                        KCU.COLUMN_NAME,
                        KCU.REFERENCED_TABLE_NAME,
                        KCU.REFERENCED_COLUMN_NAME,
                        RC.UPDATE_RULE,
                        RC.DELETE_RULE
                    FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
                    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
                        ON RC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
                    WHERE KCU.TABLE_NAME = @tableName";

                var fkParam = fkCommand.CreateParameter();
                fkParam.ParameterName = "@tableName";
                fkParam.Value = tableName;
                fkCommand.Parameters.Add(fkParam);

                using var fkReader = await fkCommand.ExecuteReaderAsync(cancellationToken);
                while (await fkReader.ReadAsync(cancellationToken))
                {
                    foreignKeys.Add(new SchemaForeignKey(
                        ConstraintName: fkReader.GetString(0),
                        ColumnName: fkReader.GetString(1),
                        ReferencedTable: fkReader.GetString(2),
                        ReferencedSchema: null,
                        ReferencedColumn: fkReader.GetString(3),
                        OnDelete: fkReader.IsDBNull(5) ? null : fkReader.GetString(5)));
                }
            }
            catch
            {
                // FK info may not be available
            }
        }
        catch
        {
            // Fall back to SQLite-style PRAGMA queries
            return await GetTableDetailSqliteAsync(connectionString, tableName, cancellationToken);
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
            PrimaryKey: primaryKey.Count > 0 ? primaryKey.AsReadOnly() : null,
            ForeignKeys: foreignKeys.Count > 0 ? foreignKeys.AsReadOnly() : null);
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await connection.CloseAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<IReadOnlyList<SchemaTableInfo>> ListTablesSqliteAsync(
        string connectionString,
        CancellationToken cancellationToken)
    {
        var tables = new List<SchemaTableInfo>();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // SQLite tables
            using var tableCmd = connection.CreateCommand();
            tableCmd.CommandText = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY name";
            using var reader = await tableCmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var name = reader.GetString(0);
                tables.Add(new SchemaTableInfo(
                    Name: name,
                    Schema: null,
                    Type: "Table",
                    ColumnCount: 0,
                    IsSystem: name == "__EFMigrationsHistory"));
            }
        }
        catch
        {
            // Unable to query
        }

        return tables.AsReadOnly();
    }

    private static async Task<SchemaTableDetail> GetTableDetailSqliteAsync(
        string connectionString,
        string tableName,
        CancellationToken cancellationToken)
    {
        var columns = new List<SchemaColumnInfo>();
        var primaryKey = new List<string>();
        var foreignKeys = new List<SchemaForeignKey>();

        try
        {
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // SQLite PRAGMA table_info
            using var pragmaCmd = connection.CreateCommand();
            pragmaCmd.CommandText = $"PRAGMA table_info(\"{tableName}\")";
            using var reader = await pragmaCmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var name = reader.GetString(1);
                var type = reader.GetString(2);
                var notNull = reader.GetBoolean(3);
                var defaultValue = reader.IsDBNull(4) ? null : reader.GetString(4);
                var isPk = reader.GetInt32(5) == 1;

                if (isPk)
                    primaryKey.Add(name);

                columns.Add(new SchemaColumnInfo(
                    Name: name,
                    DataType: type,
                    IsNullable: !notNull,
                    IsKey: isPk,
                    DefaultValue: defaultValue));
            }

            // SQLite PRAGMA foreign_key_list
            try
            {
                using var fkCmd = connection.CreateCommand();
                fkCmd.CommandText = $"PRAGMA foreign_key_list(\"{tableName}\")";
                using var fkReader = await fkCmd.ExecuteReaderAsync(cancellationToken);

                while (await fkReader.ReadAsync(cancellationToken))
                {
                    var fromCol = fkReader.GetString(3);
                    var refTable = fkReader.GetString(2);
                    var refCol = fkReader.GetString(4);
                    var onDelete = fkReader.IsDBNull(6) ? null : fkReader.GetString(6);

                    foreignKeys.Add(new SchemaForeignKey(
                        ConstraintName: $"FK_{tableName}_{refTable}_{fromCol}",
                        ColumnName: fromCol,
                        ReferencedTable: refTable,
                        ReferencedSchema: null,
                        ReferencedColumn: refCol,
                        OnDelete: onDelete));
                }
            }
            catch
            {
                // FK info not available
            }
        }
        catch
        {
            // Unable to query
        }

        // Mark PK columns
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
            PrimaryKey: primaryKey.Count > 0 ? primaryKey.AsReadOnly() : null,
            ForeignKeys: foreignKeys.Count > 0 ? foreignKeys.AsReadOnly() : null);
    }
}
