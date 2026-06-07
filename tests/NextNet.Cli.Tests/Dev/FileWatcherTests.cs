using NextNet.Cli.Dev;
using NextNet.Cli.UI;
using Xunit;

namespace NextNet.Cli.Tests.Dev;

public class FileWatcherTests
{
    [Fact]
    public void Constructor_NullAppDir_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new FileWatcher(null!, new NextNetConsole(OutputMode.Plain)));
    }

    [Fact]
    public void Constructor_NullConsole_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new FileWatcher("/tmp", null!));
    }

    [Fact]
    public void IsFullReloadRequired_LayoutFile_ReturnsTrue()
    {
        var result = InvokeIsFullReloadRequired("/app/layout.cs");
        Assert.True(result);
    }

    [Fact]
    public void IsFullReloadRequired_ConfigFile_ReturnsTrue()
    {
        var result = InvokeIsFullReloadRequired("nextnet.config.json");
        Assert.True(result);
    }

    [Fact]
    public void IsFullReloadRequired_LayoutPath_ReturnsTrue()
    {
        var result = InvokeIsFullReloadRequired("/some/deep/path/layout.cs");
        Assert.True(result);
    }

    [Fact]
    public void IsFullReloadRequired_ErrorFile_ReturnsTrue()
    {
        var result = InvokeIsFullReloadRequired("error.cs");
        Assert.True(result);
    }

    [Fact]
    public void IsFullReloadRequired_ErrorFilePath_ReturnsTrue()
    {
        var result = InvokeIsFullReloadRequired("/app/error.cs");
        Assert.True(result);
    }

    [Fact]
    public void IsFullReloadRequired_PageFile_ReturnsFalse()
    {
        var result = InvokeIsFullReloadRequired("/app/page.cs");
        Assert.False(result);
    }

    [Fact]
    public void IsFullReloadRequired_ComponentFile_ReturnsFalse()
    {
        var result = InvokeIsFullReloadRequired("/app/components/Button.cs");
        Assert.False(result);
    }

    [Fact]
    public void IsFullReloadRequired_UnknownFile_ReturnsFalse()
    {
        var result = InvokeIsFullReloadRequired("readme.md");
        Assert.False(result);
    }

    [Fact]
    public void Start_WhenDisposed_ThrowsObjectDisposed()
    {
        var watcher = new FileWatcher("/nonexistent", new NextNetConsole(OutputMode.Plain));
        watcher.Dispose();

        Assert.Throws<ObjectDisposedException>(() => watcher.Start());
    }

    [Fact]
    public void Start_WhenAppDirDoesNotExist_DoesNotThrow()
    {
        // Should write a warning but not throw
        var watcher = new FileWatcher("/nonexistent-path-12345", new NextNetConsole(OutputMode.Plain));
        watcher.Start();
        watcher.Stop();
    }

    [Fact]
    public void Stop_WithoutStart_DoesNotThrow()
    {
        var watcher = new FileWatcher("/tmp", new NextNetConsole(OutputMode.Plain));
        watcher.Stop(); // Should be a no-op
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        var watcher = new FileWatcher("/tmp", new NextNetConsole(OutputMode.Plain));
        watcher.Dispose();
        watcher.Dispose(); // Should not throw
    }

    /// <summary>
    /// Invokes the private static IsFullReloadRequired method via reflection.
    /// </summary>
    private static bool InvokeIsFullReloadRequired(string filePath)
    {
        var method = typeof(FileWatcher).GetMethod(
            "IsFullReloadRequired",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (bool)method!.Invoke(null, new object[] { filePath })!;
    }
}
