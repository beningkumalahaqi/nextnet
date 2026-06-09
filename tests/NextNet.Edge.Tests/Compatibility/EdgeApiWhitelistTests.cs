using NextNet.Edge.Compatibility;
using Xunit;

namespace NextNet.Edge.Tests.Compatibility;

public class EdgeApiWhitelistTests
{
    private readonly EdgeApiWhitelist _whitelist;

    public EdgeApiWhitelistTests()
    {
        _whitelist = new EdgeApiWhitelist();
    }

    [Theory]
    [InlineData("System.Text.Encoding")]
    [InlineData("System.Text.Json.JsonSerializer")]
    [InlineData("System.Collections.Generic.List`1")]
    [InlineData("System.Threading.Tasks.Task")]
    [InlineData("System.Threading.CancellationToken")]
    [InlineData("System.IO.MemoryStream")]
    [InlineData("System.IO.Stream")]
    [InlineData("System.Net.Http.HttpClient")]
    [InlineData("System.Linq.Enumerable")]
    [InlineData("Microsoft.AspNetCore.Http.HttpContext")]
    [InlineData("NextNet.Components.IPage")]
    [InlineData("NextNet.Edge.EdgeOptions")]
    [InlineData("NextNet.Rendering.SsrRenderer")]
    public void IsTypeAllowed_Should_ReturnTrue_When_TypeIsAllowed(string typeName)
    {
        Assert.True(_whitelist.IsTypeAllowed(typeName),
            $"Expected '{typeName}' to be allowed.");
    }

    [Theory]
    [InlineData("System.IO.File")]
    [InlineData("System.IO.FileInfo")]
    [InlineData("System.IO.Directory")]
    [InlineData("System.IO.FileStream")]
    [InlineData("System.Data.DataTable")]
    [InlineData("System.Data.DataSet")]
    [InlineData("System.Diagnostics.Process")]
    [InlineData("System.Diagnostics.ProcessStartInfo")]
    [InlineData("System.Net.Sockets.Socket")]
    [InlineData("System.Net.Sockets.TcpClient")]
    [InlineData("System.Threading.Thread")]
    [InlineData("System.Threading.ThreadPool")]
    [InlineData("System.Reflection.Emit.DynamicMethod")]
    [InlineData("System.Reflection.Emit.AssemblyBuilder")]
    public void IsTypeAllowed_Should_ReturnFalse_When_TypeIsBlocked(string typeName)
    {
        Assert.False(_whitelist.IsTypeAllowed(typeName),
            $"Expected '{typeName}' to be blocked.");
    }

    [Theory]
    [InlineData("System.IO.File", "Use in-memory storage")]
    [InlineData("System.Data.DataTable", "Use in-memory collections")]
    [InlineData("System.Diagnostics.Process", "Edge cannot start processes")]
    [InlineData("System.Net.Sockets.Socket", "Use HttpClient")]
    [InlineData("System.Threading.Thread", "Use Task.Run")]
    [InlineData("System.Reflection.Emit.DynamicMethod", "Dynamic code generation")]
    public void GetAlternative_Should_ReturnSuggestion_When_TypeIsBlocked(string typeName, string expectedPartial)
    {
        var suggestion = _whitelist.GetAlternative(typeName);
        Assert.NotNull(suggestion);
        Assert.Contains(expectedPartial, suggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetAlternative_Should_ReturnNull_When_TypeIsUnknown()
    {
        Assert.Null(_whitelist.GetAlternative("Some.Unknown.Type"));
    }

    [Fact]
    public void IsTypeAllowed_Should_ReturnTrue_When_NamespaceIsUnknown()
    {
        // Unknown namespaces should be allowed by default
        Assert.True(_whitelist.IsTypeAllowed("MyApp.MyComponent"));
        Assert.True(_whitelist.IsTypeAllowed("MyCustomLib.SomeClass"));
    }

    [Fact]
    public void IsTypeAllowed_Should_Throw_When_TypeNameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _whitelist.IsTypeAllowed(null!));
    }

    [Fact]
    public void AllowedNamespaces_Should_ContainCoreNamespaces_When_Initialized()
    {
        Assert.Contains("System.Text", _whitelist.AllowedNamespaces);
        Assert.Contains("System.Threading.Tasks", _whitelist.AllowedNamespaces);
        Assert.Contains("NextNet", _whitelist.AllowedNamespaces);
    }

    [Fact]
    public void BlockedNamespaces_Should_ContainBlockedNamespaces_When_Initialized()
    {
        Assert.Contains("System.IO.File", _whitelist.BlockedNamespaces);
        Assert.Contains("System.Data", _whitelist.BlockedNamespaces);
        Assert.Contains("System.Diagnostics.Process", _whitelist.BlockedNamespaces);
        Assert.Contains("System.Net.Sockets", _whitelist.BlockedNamespaces);
        Assert.Contains("System.Reflection.Emit", _whitelist.BlockedNamespaces);
    }
}
