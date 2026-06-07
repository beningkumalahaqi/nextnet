namespace NextNet.Edge.Compatibility.EdgeApiSubset;

/// <summary>
/// Provides detailed information about .NET APIs that are blocked on edge runtimes.
/// Organises the blocked APIs by category for easier reference.
/// </summary>
public static class BlockedApis
{
    /// <summary>
    /// Gets the set of blocked file system APIs.
    /// </summary>
    public static IReadOnlySet<string> FileSystem { get; } = new HashSet<string>
    {
        "System.IO.File",
        "System.IO.FileInfo",
        "System.IO.Directory",
        "System.IO.DirectoryInfo",
        "System.IO.DriveInfo",
        "System.IO.FileSystemWatcher",
        "System.IO.FileStream",
        "System.IO.FileLoadException",
    };

    /// <summary>
    /// Gets the set of blocked data access APIs.
    /// </summary>
    public static IReadOnlySet<string> DataAccess { get; } = new HashSet<string>
    {
        "System.Data.DataTable",
        "System.Data.DataSet",
        "System.Data.DataColumn",
        "System.Data.DataRow",
        "System.Data.Common.DbConnection",
        "System.Data.Common.DbCommand",
        "System.Data.Common.DbDataReader",
        "System.Data.SqlClient.SqlConnection",
        "System.Data.SqlClient.SqlCommand",
        "System.Data.SqlClient.SqlDataReader",
    };

    /// <summary>
    /// Gets the set of blocked process/OS APIs.
    /// </summary>
    public static IReadOnlySet<string> ProcessAndOS { get; } = new HashSet<string>
    {
        "System.Diagnostics.Process",
        "System.Diagnostics.ProcessStartInfo",
        "System.Diagnostics.ProcessModule",
        "System.Diagnostics.ProcessThread",
    };

    /// <summary>
    /// Gets the set of blocked networking APIs (socket-level).
    /// </summary>
    public static IReadOnlySet<string> SocketNetworking { get; } = new HashSet<string>
    {
        "System.Net.Sockets.Socket",
        "System.Net.Sockets.TcpClient",
        "System.Net.Sockets.TcpListener",
        "System.Net.Sockets.UdpClient",
        "System.Net.Sockets.NetworkStream",
        "System.Net.Sockets.SocketAsyncEventArgs",
    };

    /// <summary>
    /// Gets the set of blocked threading APIs.
    /// </summary>
    public static IReadOnlySet<string> Threading { get; } = new HashSet<string>
    {
        "System.Threading.Thread",
        "System.Threading.ThreadPool",
        "System.Threading.Timer",
        "System.Threading.Tasks.Parallel",
    };

    /// <summary>
    /// Gets the set of blocked reflection APIs.
    /// </summary>
    public static IReadOnlySet<string> ReflectionLimited { get; } = new HashSet<string>
    {
        "System.Reflection.Assembly",
        "System.Reflection.Emit.AssemblyBuilder",
        "System.Reflection.Emit.DynamicMethod",
        "System.Reflection.Emit.TypeBuilder",
        "System.Reflection.Emit.ILGenerator",
        "System.Reflection.Emit.ModuleBuilder",
        "System.Reflection.Context.CustomReflectionContext",
    };

    /// <summary>
    /// Gets all blocked types across all categories.
    /// </summary>
    public static IReadOnlySet<string> All { get; } = new HashSet<string>(
        FileSystem
            .Concat(DataAccess)
            .Concat(ProcessAndOS)
            .Concat(SocketNetworking)
            .Concat(Threading)
            .Concat(ReflectionLimited));
}
