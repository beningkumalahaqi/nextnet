using NextNet.Configuration;
using NextNet.Isr.Configuration;
using NextNet.Isr.Manifest;

namespace NextNet.Isr.Tests;

public class IsrConfigValidatorTests
{
    private readonly IsrGlobalOptions _validGlobalOptions;
    private readonly Manifest.IsrManifest _emptyManifest;

    public IsrConfigValidatorTests()
    {
        _validGlobalOptions = new IsrGlobalOptions
        {
            DefaultRevalidateSeconds = 60,
            MaxConcurrentRegenerations = 4,
            MaxPendingRevalidations = 100,
            DeduplicationWindowSeconds = 30
        };

        _emptyManifest = Manifest.IsrManifest.Empty;
    }

    [Fact]
    public void Validate_WithValidConfig_ReturnsNoErrors()
    {
        var errors = IsrConfigValidator.Validate(_validGlobalOptions, _emptyManifest);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_WithNegativeDefaultRevalidate_ReturnsError()
    {
        var options = new IsrGlobalOptions { DefaultRevalidateSeconds = -1 };
        var errors = IsrConfigValidator.Validate(options, _emptyManifest);

        Assert.Contains(errors, e =>
            e.Path != null && e.Path.Contains("DefaultRevalidateSeconds"));
    }

    [Fact]
    public void Validate_WithZeroMaxConcurrent_ReturnsError()
    {
        var options = new IsrGlobalOptions { MaxConcurrentRegenerations = 0 };
        var errors = IsrConfigValidator.Validate(options, _emptyManifest);

        Assert.Contains(errors, e =>
            e.Path != null && e.Path.Contains("MaxConcurrentRegenerations"));
    }

    [Fact]
    public void Validate_WithZeroMaxPending_ReturnsError()
    {
        var options = new IsrGlobalOptions { MaxPendingRevalidations = 0 };
        var errors = IsrConfigValidator.Validate(options, _emptyManifest);

        Assert.Contains(errors, e =>
            e.Path != null && e.Path.Contains("MaxPendingRevalidations"));
    }

    [Fact]
    public void Validate_WithNegativeDeduplicationWindow_ReturnsError()
    {
        var options = new IsrGlobalOptions { DeduplicationWindowSeconds = -1 };
        var errors = IsrConfigValidator.Validate(options, _emptyManifest);

        Assert.Contains(errors, e =>
            e.Path != null && e.Path.Contains("DeduplicationWindowSeconds"));
    }

    [Fact]
    public void Validate_WithInvalidRouteMetadata_ReturnsError()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/test"] = new() { RevalidateSeconds = -1 }
        };
        var manifest = new Manifest.IsrManifest(routes, _validGlobalOptions);

        var errors = IsrConfigValidator.Validate(_validGlobalOptions, manifest);

        Assert.Contains(errors, e =>
            e.Path != null && e.Path.Contains("RevalidateSeconds") && e.Path.Contains("/test"));
    }

    [Fact]
    public void Validate_NullOptions_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IsrConfigValidator.Validate(null!, _emptyManifest));
    }

    [Fact]
    public void Validate_NullManifest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IsrConfigValidator.Validate(_validGlobalOptions, null!));
    }
}
