using Xunit;

namespace NextNet.DevTools.Tests;

public class DevToolsEventTests
{
    [Fact]
    public void RouteDiscoveredEvent_HasProperties()
    {
        var evt = new RouteDiscoveredEvent
        {
            Path = "/test",
            Type = "static",
            File = "app/test/page.cs"
        };

        Assert.Equal("/test", evt.Path);
        Assert.Equal("static", evt.Type);
        Assert.Equal("app/test/page.cs", evt.File);
    }

    [Fact]
    public void ComponentRenderedEvent_HasProperties()
    {
        var evt = new ComponentRenderedEvent
        {
            Component = "Header",
            DurationMs = 42,
            Route = "/"
        };

        Assert.Equal("Header", evt.Component);
        Assert.Equal(42, evt.DurationMs);
        Assert.Equal("/", evt.Route);
    }

    [Fact]
    public void BuildCompletedEvent_HasSteps()
    {
        var evt = new BuildCompletedEvent
        {
            TotalDurationMs = 1000,
            Success = true,
            Steps = new[]
            {
                new BuildStepMetric { Name = "Compile", DurationMs = 500 },
                new BuildStepMetric { Name = "Bundle", DurationMs = 300 }
            }
        };

        Assert.Equal(1000, evt.TotalDurationMs);
        Assert.True(evt.Success);
        Assert.Equal(2, evt.Steps.Count);
    }

    [Fact]
    public void HmrUpdatedEvent_HasProperties()
    {
        var evt = new HmrUpdatedEvent
        {
            Files = new[] { "app/page.cs", "app/layout.cs" },
            DurationMs = 420,
            Success = true
        };

        Assert.Equal(2, evt.Files.Count);
        Assert.Contains("app/page.cs", evt.Files);
        Assert.Equal(420, evt.DurationMs);
    }

    [Fact]
    public void ErrorOccurredEvent_HasProperties()
    {
        var evt = new ErrorOccurredEvent
        {
            Message = "Something went wrong",
            File = "app/page.cs",
            Line = 42
        };

        Assert.Equal("Something went wrong", evt.Message);
        Assert.Equal("app/page.cs", evt.File);
        Assert.Equal(42, evt.Line);
    }

    [Fact]
    public void BuildStepMetric_HasProperties()
    {
        var metric = new BuildStepMetric
        {
            Name = "Test step",
            DurationMs = 123
        };

        Assert.Equal("Test step", metric.Name);
        Assert.Equal(123, metric.DurationMs);
    }
}
