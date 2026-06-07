namespace NextNet.Data.Sqlite.Tests.Fixtures;

/// <summary>
/// Creates a temp directory for SQLite file-based tests.
/// The directory and all files are cleaned up on disposal.
/// </summary>
public sealed class SqliteTestFixture : IDisposable
{
    /// <summary>
    /// Gets the path to the temporary directory.
    /// </summary>
    public string TempDir { get; }

    /// <summary>
    /// Gets the default database file path within the temp directory.
    /// </summary>
    public string DefaultDbPath => Path.Combine(TempDir, "test.db");

    /// <summary>
    /// Initializes a new instance and creates the temp directory.
    /// </summary>
    public SqliteTestFixture()
    {
        TempDir = Path.Combine(Path.GetTempPath(), "NextNetSqliteTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempDir);
    }

    /// <summary>
    /// Cleans up the temp directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }
}
