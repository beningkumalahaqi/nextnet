using NextNet.Edge.Compatibility.EdgeApiSubset;
using Xunit;

namespace NextNet.Edge.Tests.Compatibility.EdgeApiSubset;

public class AlternativeApisTests
{
    [Theory]
    [InlineData("System.IO.File", "in-memory storage")]
    [InlineData("System.Data.DataTable", "in-memory collections")]
    [InlineData("System.Diagnostics.Process", "cannot start processes")]
    [InlineData("System.Net.Sockets.Socket", "HttpClient")]
    [InlineData("System.Threading.Thread", "Task.Run")]
    [InlineData("System.Reflection.Emit.DynamicMethod", "Dynamic code generation")]
    public void GetSuggestion_KnownType_ReturnsSuggestion(string typeName, string expectedPartial)
    {
        var suggestion = AlternativeApis.GetSuggestion(typeName);
        Assert.NotNull(suggestion);
        Assert.Contains(expectedPartial, suggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetSuggestion_UnknownType_ReturnsNull()
    {
        Assert.Null(AlternativeApis.GetSuggestion("Some.Unknown.Type"));
    }
}
