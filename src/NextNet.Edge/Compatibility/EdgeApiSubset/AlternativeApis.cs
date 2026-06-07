namespace NextNet.Edge.Compatibility.EdgeApiSubset;

/// <summary>
/// Provides alternative API recommendations for types that are blocked on edge runtimes.
/// </summary>
public static class AlternativeApis
{
    /// <summary>
    /// Gets a suggested alternative for the specified blocked type.
    /// </summary>
    /// <param name="blockedType">The fully-qualified blocked type name.</param>
    /// <returns>The recommended alternative, or null if no suggestion is available.</returns>
    public static string? GetSuggestion(string blockedType)
    {
        if (Alternatives.TryGetValue(blockedType, out var suggestion))
            return suggestion;

        // Check namespace-level alternatives
        var lastDot = blockedType.LastIndexOf('.');
        if (lastDot > 0)
        {
            var ns = blockedType[..lastDot];
            if (NamespaceAlternatives.TryGetValue(ns, out var nsSuggestion))
                return nsSuggestion;
        }

        return null;
    }

    /// <summary>
    /// Maps blocked types to their recommended alternative APIs.
    /// </summary>
    private static readonly Dictionary<string, string> Alternatives = new(StringComparer.Ordinal)
    {
        // File system
        ["System.IO.File"] = "Use in-memory storage (MemoryStream) or edge storage (R2, S3 via HttpClient)",
        ["System.IO.FileInfo"] = "Use in-memory storage (MemoryStream) or edge storage APIs",
        ["System.IO.Directory"] = "Edge has no file system directory access; pre-bundle assets at build time",
        ["System.IO.DirectoryInfo"] = "Edge has no file system directory access",
        ["System.IO.FileStream"] = "Use MemoryStream for in-memory operations",
        ["System.IO.DriveInfo"] = "Drive information is not available on edge runtimes",
        ["System.IO.FileSystemWatcher"] = "File system watchers are not available on edge runtimes",
        ["System.IO.FileLoadException"] = "Avoid file loading on edge; bundle assemblies at build time",

        // Data access
        ["System.Data.DataTable"] = "Use in-memory collections (List<T>, Dictionary<K,V>)",
        ["System.Data.DataSet"] = "Use in-memory collections (List<T>, Dictionary<K,V>)",
        ["System.Data.DataColumn"] = "Use typed objects or records",
        ["System.Data.DataRow"] = "Use typed objects or records",
        ["System.Data.Common.DbConnection"] = "Use HTTP-based database APIs (REST/gRPC) via HttpClient",
        ["System.Data.Common.DbCommand"] = "Use HTTP-based database APIs (REST/gRPC) via HttpClient",
        ["System.Data.Common.DbDataReader"] = "Use HTTP-based database APIs (REST/gRPC) via HttpClient",
        ["System.Data.SqlClient.SqlConnection"] = "Use HTTP-based database APIs (REST/gRPC) via HttpClient",

        // Process and OS
        ["System.Diagnostics.Process"] = "Edge cannot start processes; use background tasks or external APIs",
        ["System.Diagnostics.ProcessStartInfo"] = "Edge cannot start processes",

        // Socket networking
        ["System.Net.Sockets.Socket"] = "Use HttpClient for HTTP-based communication",
        ["System.Net.Sockets.TcpClient"] = "Use HttpClient for HTTP-based communication",
        ["System.Net.Sockets.TcpListener"] = "Edge cannot listen on sockets; use HttpClient outbound only",
        ["System.Net.Sockets.UdpClient"] = "Edge does not support UDP; use HTTP-based APIs",
        ["System.Net.Sockets.NetworkStream"] = "Use HttpClient for HTTP-based communication",

        // Threading
        ["System.Threading.Thread"] = "Use Task.Run() or async/await patterns",
        ["System.Threading.ThreadPool"] = "Use Task-based async patterns instead",
        ["System.Threading.Timer"] = "Use CancellationTokenSource.CancelAfter or Task.Delay",
        ["System.Threading.Tasks.Parallel"] = "Use async Task-based concurrency patterns",

        // Reflection
        ["System.Reflection.Assembly"] = "Use source generators at build time instead of runtime reflection",
        ["System.Reflection.Emit.AssemblyBuilder"] = "Dynamic code generation is not available on edge",
        ["System.Reflection.Emit.DynamicMethod"] = "Dynamic code generation is not available on edge",
        ["System.Reflection.Emit.TypeBuilder"] = "Dynamic code generation is not available on edge",
        ["System.Reflection.Emit.ILGenerator"] = "Dynamic code generation is not available on edge",
        ["System.Reflection.Emit.ModuleBuilder"] = "Dynamic code generation is not available on edge",
        ["System.Reflection.Context.CustomReflectionContext"] = "Custom reflection contexts are not supported on edge",
    };

    /// <summary>
    /// Maps blocked namespaces to general alternative suggestions.
    /// </summary>
    private static readonly Dictionary<string, string> NamespaceAlternatives = new(StringComparer.Ordinal)
    {
        ["System.IO"] = "Use in-memory streams (MemoryStream) instead of file I/O",
        ["System.Data"] = "Use in-memory collections (List<T>, Dictionary<K,V>) or HTTP-based data APIs",
        ["System.Data.SqlClient"] = "Use HTTP-based database APIs via HttpClient",
        ["System.Diagnostics"] = "Use logging abstractions (Microsoft.Extensions.Logging) instead",
        ["System.Net.Sockets"] = "Use HttpClient for outbound HTTP communication",
        ["System.Threading"] = "Use async/await task-based patterns",
        ["System.Reflection.Emit"] = "Use source generators at build time instead of runtime code generation",
        ["System.Reflection"] = "Limit reflection to read-only operations; avoid emit and assembly loading",
        ["System.Drawing"] = "Use skia-sharp or image processing via HTTP APIs",
    };
}
