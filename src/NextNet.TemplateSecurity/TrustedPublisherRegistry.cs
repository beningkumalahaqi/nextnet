namespace NextNet.TemplateSecurity;

using System.Text.Json;

/// <summary>
/// Manages the trusted publisher registry, including adding, removing, and revoking publishers.
/// Persists the registry to a JSON file on disk.
/// </summary>
public sealed class TrustedPublisherRegistry
{
    private readonly SecurityOptions _options;
    private readonly List<TrustedPublisher> _trusted = new();
    private readonly List<RevokedPublisher> _revoked = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="TrustedPublisherRegistry"/> class.
    /// </summary>
    /// <param name="options">Security options containing the path to the registry file.</param>
    public TrustedPublisherRegistry(SecurityOptions options) => _options = options;
    
    /// <summary>
    /// Gets the current list of trusted publishers.
    /// </summary>
    public IReadOnlyList<TrustedPublisher> Trusted => _trusted;

    /// <summary>
    /// Gets the current list of revoked publishers.
    /// </summary>
    public IReadOnlyList<RevokedPublisher> Revoked => _revoked;

    /// <summary>
    /// Loads the trusted publisher registry from disk.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_options.TrustedPublishersFile)) return;
        try
        {
            var json = await File.ReadAllTextAsync(_options.TrustedPublishersFile, ct);
            var data = JsonSerializer.Deserialize<TrustedPublishersFile>(json, JsonOptions);
            if (data is not null)
            {
                _trusted.Clear();
                _trusted.AddRange(data.Trusted);
                _revoked.Clear();
                _revoked.AddRange(data.Revoked);
            }
        }
        catch
        {
            // Ignore corrupted file
        }
    }

    /// <summary>
    /// Checks whether a publisher is in the trusted list.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <returns><c>true</c> if the publisher is trusted; otherwise <c>false</c>.</returns>
    public bool IsTrusted(string publisherId)
    {
        return _trusted.Any(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Checks whether a publisher has been revoked.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <returns><c>true</c> if the publisher has been revoked; otherwise <c>false</c>.</returns>
    public bool IsRevoked(string publisherId)
    {
        return _revoked.Any(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
    }

    /// <summary>
    /// Adds a publisher to the trusted list.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <param name="displayName">Optional display name.</param>
    /// <param name="publicKey">Optional public key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="publisherId"/> is null or whitespace.</exception>
    public async Task AddTrustedAsync(string publisherId, string? displayName = null, string? publicKey = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publisherId);
        
        _trusted.RemoveAll(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
        _trusted.Add(new TrustedPublisher
        {
            PublisherId = publisherId,
            DisplayName = displayName,
            PublicKey = publicKey,
            AddedAt = DateTime.UtcNow
        });
        await SaveAsync(ct);
    }

    /// <summary>
    /// Removes a publisher from the trusted list.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task RemoveTrustedAsync(string publisherId, CancellationToken ct = default)
    {
        _trusted.RemoveAll(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
        await SaveAsync(ct);
    }

    /// <summary>
    /// Revokes a publisher, removing them from the trusted list and adding them to the revoked list.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <param name="reason">Optional reason for revocation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="publisherId"/> is null or whitespace.</exception>
    public async Task RevokeAsync(string publisherId, string? reason = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publisherId);
        _revoked.RemoveAll(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
        _revoked.Add(new RevokedPublisher
        {
            PublisherId = publisherId,
            Reason = reason,
            RevokedAt = DateTime.UtcNow
        });
        _trusted.RemoveAll(p => string.Equals(p.PublisherId, publisherId, StringComparison.Ordinal));
        await SaveAsync(ct);
    }

    /// <summary>
    /// Saves the current trusted and revoked publisher lists to disk.
    /// Uses atomic write via a temporary file.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task SaveAsync(CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(_options.TrustedPublishersFile)!;
        Directory.CreateDirectory(dir);
        var data = new TrustedPublishersFile { Trusted = _trusted, Revoked = _revoked };
        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = _options.TrustedPublishersFile + ".tmp";
        await File.WriteAllTextAsync(tempPath, json, ct);
        File.Move(tempPath, _options.TrustedPublishersFile, overwrite: true);
    }

    private sealed class TrustedPublishersFile
    {
        public List<TrustedPublisher> Trusted { get; set; } = new();
        public List<RevokedPublisher> Revoked { get; set; } = new();
    }
}
