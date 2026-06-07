using NextNet.Edge.Compatibility.EdgeApiSubset;
using Xunit;

namespace NextNet.Edge.Tests.Compatibility.EdgeApiSubset;

public class BlockedApisTests
{
    [Fact]
    public void FileSystem_ContainsBlockedTypes()
    {
        Assert.Contains("System.IO.File", BlockedApis.FileSystem);
        Assert.Contains("System.IO.Directory", BlockedApis.FileSystem);
        Assert.Contains("System.IO.FileStream", BlockedApis.FileSystem);
    }

    [Fact]
    public void DataAccess_ContainsBlockedTypes()
    {
        Assert.Contains("System.Data.DataTable", BlockedApis.DataAccess);
        Assert.Contains("System.Data.SqlClient.SqlConnection", BlockedApis.DataAccess);
    }

    [Fact]
    public void ProcessAndOS_ContainsBlockedTypes()
    {
        Assert.Contains("System.Diagnostics.Process", BlockedApis.ProcessAndOS);
    }

    [Fact]
    public void SocketNetworking_ContainsBlockedTypes()
    {
        Assert.Contains("System.Net.Sockets.Socket", BlockedApis.SocketNetworking);
        Assert.Contains("System.Net.Sockets.TcpClient", BlockedApis.SocketNetworking);
    }

    [Fact]
    public void Threading_ContainsBlockedTypes()
    {
        Assert.Contains("System.Threading.Thread", BlockedApis.Threading);
        Assert.Contains("System.Threading.ThreadPool", BlockedApis.Threading);
    }

    [Fact]
    public void ReflectionLimited_ContainsBlockedTypes()
    {
        Assert.Contains("System.Reflection.Emit.DynamicMethod", BlockedApis.ReflectionLimited);
        Assert.Contains("System.Reflection.Emit.AssemblyBuilder", BlockedApis.ReflectionLimited);
    }

    [Fact]
    public void All_ContainsAllCategories()
    {
        Assert.True(BlockedApis.All.Count > 0);
        Assert.Contains("System.IO.File", BlockedApis.All);
        Assert.Contains("System.Diagnostics.Process", BlockedApis.All);
        Assert.Contains("System.Reflection.Emit.DynamicMethod", BlockedApis.All);
    }
}
