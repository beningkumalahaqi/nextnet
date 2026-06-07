using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

/// <summary>
/// Tests for internal members of <see cref="RouteFileWatcher"/>.
/// These tests verify the skip logic, event queuing, and debounce behavior
/// without requiring real file system I/O.
/// </summary>
public class RouteFileWatcherInternalTests
{
    [Theory]
    [InlineData("page.cs", false)]
    [InlineData("layout.cs", false)]
    [InlineData("route.cs", false)]
    [InlineData("error.cs", false)]
    [InlineData("helper.g.cs", true)]
    [InlineData("component.razor.g.cs", true)]
    [InlineData("generated.g.cs", true)]
    [InlineData("somefile.razor.g.cs", true)]
    public void ShouldSkipFile_VariousFileNames_ReturnsExpected(string fileName, bool expected)
    {
        var result = RouteFileWatcher.ShouldSkipFile(fileName);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void QueueEvent_WithDebounce_InvokesOnChanged()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var receivedEvents = new List<FileChangeEvent>();
            using var watcher = new RouteFileWatcher(tempDir, debounceMilliseconds: 50);

            watcher.OnChanged += e => receivedEvents.Add(e);

            watcher.Start();

            // Queue events directly
            watcher.QueueEvent("/test/file.cs", FileChangeType.Created);
            watcher.QueueEvent("/test/file.cs", FileChangeType.Modified);

            // Wait for debounce to fire
            Thread.Sleep(200);

            Assert.NotEmpty(receivedEvents);
            // The last event for /test/file.cs should be Modified
            var lastEvent = receivedEvents.Last();
            Assert.Equal(FileChangeType.Modified, lastEvent.ChangeType);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void QueueEvent_MultipleFiles_AllReceived()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var receivedEvents = new List<FileChangeEvent>();
            using var watcher = new RouteFileWatcher(tempDir, debounceMilliseconds: 50);

            watcher.OnChanged += e => receivedEvents.Add(e);

            watcher.Start();

            watcher.QueueEvent("/test/file1.cs", FileChangeType.Created);
            watcher.QueueEvent("/test/file2.cs", FileChangeType.Modified);
            watcher.QueueEvent("/test/file3.cs", FileChangeType.Deleted);

            Thread.Sleep(200);

            Assert.Equal(3, receivedEvents.Count);
            Assert.Contains(receivedEvents, e => e.FilePath == "/test/file1.cs" && e.ChangeType == FileChangeType.Created);
            Assert.Contains(receivedEvents, e => e.FilePath == "/test/file2.cs" && e.ChangeType == FileChangeType.Modified);
            Assert.Contains(receivedEvents, e => e.FilePath == "/test/file3.cs" && e.ChangeType == FileChangeType.Deleted);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void QueueEvent_SameFileMultipleTimes_LastEventWins()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var receivedEvents = new List<FileChangeEvent>();
            using var watcher = new RouteFileWatcher(tempDir, debounceMilliseconds: 50);

            watcher.OnChanged += e => receivedEvents.Add(e);

            watcher.Start();

            // Queue create, then modify, then delete for same file
            watcher.QueueEvent("/test/file.cs", FileChangeType.Created);
            watcher.QueueEvent("/test/file.cs", FileChangeType.Modified);
            watcher.QueueEvent("/test/file.cs", FileChangeType.Deleted);

            Thread.Sleep(200);

            // Should only fire one event for this file: Deleted
            var fileEvents = receivedEvents.Where(e => e.FilePath == "/test/file.cs").ToList();
            Assert.Single(fileEvents);
            Assert.Equal(FileChangeType.Deleted, fileEvents[0].ChangeType);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void OnChangedEventHandler_ExceptionDoesNotCrash()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            using var watcher = new RouteFileWatcher(tempDir, debounceMilliseconds: 50);

            // Register a handler that throws
            watcher.OnChanged += e => throw new InvalidOperationException("Test exception");

            watcher.Start();

            // This should not throw
            watcher.QueueEvent("/test/file.cs", FileChangeType.Created);

            Thread.Sleep(200);
            // If we get here without exception, the handler was caught
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void FirePendingEvents_WithNoHandlers_DoesNotThrow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            using var watcher = new RouteFileWatcher(tempDir, debounceMilliseconds: 50);

            watcher.Start();

            // Queue event without registering a handler - should not throw
            watcher.QueueEvent("/test/file.cs", FileChangeType.Created);

            Thread.Sleep(200);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
