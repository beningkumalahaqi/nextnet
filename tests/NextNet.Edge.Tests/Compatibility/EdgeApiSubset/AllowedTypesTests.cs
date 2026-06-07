using NextNet.Edge.Compatibility.EdgeApiSubset;
using Xunit;

namespace NextNet.Edge.Tests.Compatibility.EdgeApiSubset;

public class AllowedTypesTests
{
    [Fact]
    public void Serialization_ContainsCoreTypes()
    {
        Assert.Contains("System.Text.Json.JsonSerializer", AllowedTypes.Serialization);
        Assert.Contains("System.Text.Encoding", AllowedTypes.Serialization);
        Assert.Contains("System.Text.StringBuilder", AllowedTypes.Serialization);
    }

    [Fact]
    public void Collections_ContainsCoreTypes()
    {
        Assert.Contains("System.Collections.Generic.List`1", AllowedTypes.Collections);
        Assert.Contains("System.Collections.Generic.Dictionary`2", AllowedTypes.Collections);
    }

    [Fact]
    public void Async_ContainsCoreTypes()
    {
        Assert.Contains("System.Threading.Tasks.Task", AllowedTypes.Async);
        Assert.Contains("System.Threading.CancellationToken", AllowedTypes.Async);
    }

    [Fact]
    public void MemoryIO_ContainsCoreTypes()
    {
        Assert.Contains("System.IO.MemoryStream", AllowedTypes.MemoryIO);
        Assert.Contains("System.IO.Stream", AllowedTypes.MemoryIO);
    }

    [Fact]
    public void Networking_ContainsCoreTypes()
    {
        Assert.Contains("System.Net.Http.HttpClient", AllowedTypes.Networking);
    }

    [Fact]
    public void All_ContainsAllCategories()
    {
        Assert.True(AllowedTypes.All.Count > 0);
        Assert.Contains("System.Text.Json.JsonSerializer", AllowedTypes.All);
        Assert.Contains("System.IO.MemoryStream", AllowedTypes.All);
        Assert.Contains("System.Net.Http.HttpClient", AllowedTypes.All);
    }
}
