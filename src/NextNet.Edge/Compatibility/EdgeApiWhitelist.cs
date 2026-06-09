namespace NextNet.Edge.Compatibility;

/// <summary>
/// Defines which .NET APIs are allowed and blocked on edge runtimes (V8/WASM).
/// Provides a centralized whitelist/blocklist for the compatibility checker.
/// </summary>
public sealed class EdgeApiWhitelist
{
    /// <summary>
    /// Initializes a new instance of <see cref="EdgeApiWhitelist"/>.
    /// Populates the default allowed and blocked API sets.
    /// </summary>
    public EdgeApiWhitelist()
    {
        AllowedNamespaces = new HashSet<string>(StringComparer.Ordinal)
        {
            "System.Text",
            "System.Text.Json",
            "System.Text.RegularExpressions",
            "System.Collections",
            "System.Collections.Generic",
            "System.Collections.Concurrent",
            "System.Threading",
            "System.Threading.Tasks",
            "System.Linq",
            "System.Runtime.CompilerServices",
            "System.Runtime.InteropServices",
            "System.Buffers",
            "System.IO",
            "System.IO.Compression",
            "System.IO.Pipelines",
            "System.Net",
            "System.Net.Http",
            "System.Net.Http.Json",
            "System.Numerics",
            "System.Diagnostics",
            "System.Diagnostics.CodeAnalysis",
            "System.Diagnostics.Tracing",
            "Microsoft.AspNetCore.Http",
            "Microsoft.AspNetCore.Http.Abstractions",
            "Microsoft.AspNetCore.Http.Extensions",
            "Microsoft.AspNetCore.Routing",
            "Microsoft.AspNetCore.Routing.Patterns",
            "NextNet",
            "NextNet.Components",
            "NextNet.Configuration",
            "NextNet.Edge",
            "NextNet.Edge.Adapters",
            "NextNet.Edge.Compatibility",
            "NextNet.Edge.Middleware",
            "NextNet.Edge.Streaming",
            "NextNet.Layouts",
            "NextNet.Middleware",
            "NextNet.Rendering",
            "NextNet.Routing",
            "NextNet.Routing.Models",
            "NextNet.ServerActions",
            "NextNet.Logging",
        };

        AllowedTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            // System.IO - in-memory only
            "System.IO.Stream",
            "System.IO.MemoryStream",
            "System.IO.TextReader",
            "System.IO.TextWriter",
            "System.IO.StringReader",
            "System.IO.StringWriter",
            "System.IO.StreamReader",
            "System.IO.StreamWriter",

            // System.Threading
            "System.Threading.CancellationToken",
            "System.Threading.CancellationTokenSource",
            "System.Threading.Tasks.Task",
            "System.Threading.Tasks.Task`1",
            "System.Threading.Tasks.ValueTask",
            "System.Threading.Tasks.ValueTask`1",

            // System.Net
            "System.Net.Http.HttpClient",
            "System.Net.Http.HttpResponseMessage",
            "System.Net.Http.HttpRequestMessage",
            "System.Net.Http.Headers.HttpHeaders",
        };

        BlockedNamespaces = new HashSet<string>(StringComparer.Ordinal)
        {
            "System.IO.File",
            "System.IO.FileSystem",
            "System.IO.Directory",
            "System.Data",
            "System.Data.Common",
            "System.Data.SqlClient",
            "System.Data.SqlTypes",
            "System.Diagnostics.Process",
            "System.Diagnostics.PerformanceData",
            "System.Net.Sockets",
            "System.Net.Mail",
            "System.Net.NetworkInformation",
            "System.Net.WebSockets",
            "System.Threading.Thread",
            "System.Threading.Channels",
            "System.Reflection.Emit",
            "System.Reflection.Context",
            "System.Drawing",
            "System.Drawing.Imaging",
            "System.Drawing.Printing",
            "System.Security.Cryptography.Xml",
            "System.Security.Cryptography.Pkcs",
            "System.Xml.Serialization",
            "System.Xml.Xsl",
            "System.Xml.XPath.XDocument",
            "System.ComponentModel.Design",
            "System.ComponentModel.DataAnnotations",
            "System.Windows.Forms",
            "Microsoft.Win32",
            "Microsoft.AspNetCore.Server.Kestrel",
            "Microsoft.AspNetCore.Server.IIS",
            "Microsoft.AspNetCore.Server.HttpSys",
            "Microsoft.AspNetCore.DataProtection",
        };

        BlockedTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            // File system
            "System.IO.File",
            "System.IO.FileInfo",
            "System.IO.Directory",
            "System.IO.DirectoryInfo",
            "System.IO.DriveInfo",
            "System.IO.FileSystemWatcher",
            "System.IO.FileStream",
            "System.IO.FileLoadException",

            // Data
            "System.Data.DataTable",
            "System.Data.DataSet",
            "System.Data.SqlClient.SqlConnection",

            // Process
            "System.Diagnostics.Process",
            "System.Diagnostics.ProcessStartInfo",

            // Sockets
            "System.Net.Sockets.Socket",
            "System.Net.Sockets.TcpClient",
            "System.Net.Sockets.TcpListener",
            "System.Net.Sockets.UdpClient",

            // Thread
            "System.Threading.Thread",
            "System.Threading.ThreadPool",
            "System.Threading.Timer",
            "System.Threading.Tasks.Parallel",

            // Reflection
            "System.Reflection.Assembly",
            "System.Reflection.Emit.AssemblyBuilder",
            "System.Reflection.Emit.DynamicMethod",
            "System.Reflection.Emit.TypeBuilder",
        };
    }

    /// <summary>
    /// Gets the set of allowed namespaces that can be safely used on edge.
    /// </summary>
    public HashSet<string> AllowedNamespaces { get; }

    /// <summary>
    /// Gets the set of explicitly allowed types.
    /// </summary>
    public HashSet<string> AllowedTypes { get; }

    /// <summary>
    /// Gets the set of blocked namespaces that cannot be used on edge.
    /// </summary>
    public HashSet<string> BlockedNamespaces { get; }

    /// <summary>
    /// Gets the set of explicitly blocked types.
    /// </summary>
    public HashSet<string> BlockedTypes { get; }

    /// <summary>
    /// Checks whether the given fully-qualified type name is allowed on edge.
    /// </summary>
    /// <param name="fullTypeName">The fully-qualified type name (e.g., "System.IO.File").</param>
    /// <returns><c>true</c> if the type is allowed on edge; otherwise <c>false</c>.</returns>
    public bool IsTypeAllowed(string fullTypeName)
    {
        if (fullTypeName == null) throw new ArgumentNullException(nameof(fullTypeName));

        // Check explicit blocked types first
        if (BlockedTypes.Contains(fullTypeName))
            return false;

        // Check explicit allowed types
        if (AllowedTypes.Contains(fullTypeName))
            return true;

        // Determine the namespace from the type name
        var lastDot = fullTypeName.LastIndexOf('.');
        if (lastDot < 0)
            return true; // No namespace — assume allowed

        var ns = fullTypeName[..lastDot];

        // Check blocked namespaces
        foreach (var blocked in BlockedNamespaces)
        {
            if (ns.Equals(blocked, StringComparison.Ordinal) ||
                ns.StartsWith(blocked + ".", StringComparison.Ordinal))
                return false;
        }

        // Check allowed namespaces
        foreach (var allowed in AllowedNamespaces)
        {
            if (ns.Equals(allowed, StringComparison.Ordinal) ||
                ns.StartsWith(allowed + ".", StringComparison.Ordinal))
                return true;
        }

        // Unknown namespace — in strict mode this would be blocked, but
        // by default we allow it since it may be a user-defined namespace
        return true;
    }

    /// <summary>
    /// Gets an alternative API suggestion for a blocked type, if available.
    /// </summary>
    /// <param name="blockedType">The blocked type name.</param>
    /// <returns>The suggested alternative API, or null if no suggestion exists.</returns>
    public string? GetAlternative(string blockedType)
    {
        return blockedType switch
        {
            "System.IO.File" => "Use in-memory storage (MemoryStream) or edge storage APIs (R2, S3 via HttpClient)",
            "System.IO.FileInfo" => "Use in-memory storage (MemoryStream) or edge storage APIs",
            "System.IO.Directory" => "Edge has no file system directory access; pre-bundle assets at build time",
            "System.IO.DirectoryInfo" => "Edge has no file system directory access",
            "System.IO.FileStream" => "Use MemoryStream for in-memory operations",
            "System.IO.FileSystemWatcher" => "File system watchers are not available on edge",
            "System.Data.DataTable" => "Use in-memory collections (List<T>, Dictionary<K,V>)",
            "System.Data.DataSet" => "Use in-memory collections (List<T>, Dictionary<K,V>)",
            "System.Data.SqlClient.SqlConnection" => "Use HTTP-based database APIs (REST/gRPC) via HttpClient",
            "System.Diagnostics.Process" => "Edge cannot start processes; use background tasks or external APIs",
            "System.Diagnostics.ProcessStartInfo" => "Edge cannot start processes",
            "System.Net.Sockets.Socket" => "Use HttpClient for HTTP-based communication",
            "System.Net.Sockets.TcpClient" => "Use HttpClient for HTTP-based communication",
            "System.Net.Sockets.TcpListener" => "Edge cannot listen on sockets; use HttpClient outbound only",
            "System.Net.Sockets.UdpClient" => "Edge does not support UDP; use HTTP-based APIs",
            "System.Threading.Thread" => "Use Task.Run() or ValueTask for async operations",
            "System.Threading.ThreadPool" => "Use Task-based async patterns instead",
            "System.Threading.Timer" => "Use CancellationTokenSource.CancelAfter or Task.Delay",
            "System.Threading.Tasks.Parallel" => "Use async Task-based concurrency patterns",
            "System.Reflection.Assembly" => "Use source generators at build time instead of runtime reflection",
            "System.Reflection.Emit.AssemblyBuilder" => "Dynamic code generation is not available on edge",
            "System.Reflection.Emit.DynamicMethod" => "Dynamic code generation is not available on edge",
            _ => null,
        };
    }
}
