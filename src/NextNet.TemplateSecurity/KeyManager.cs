namespace NextNet.TemplateSecurity;

using System.Security.Cryptography;
using System.Text.Json;

/// <summary>
/// Manages cryptographic key generation, storage, and retrieval for template publishers.
/// Keys are stored as JSON files in the user's .nextnet/keys directory.
/// </summary>
public sealed class KeyManager
{
    private readonly string _keysDirectory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyManager"/> class.
    /// Creates the keys directory if it does not exist.
    /// </summary>
    public KeyManager()
    {
        _keysDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".nextnet", "keys");
        Directory.CreateDirectory(_keysDirectory);
    }
    
    /// <summary>
    /// Gets the directory path where keys are stored.
    /// </summary>
    public string KeysDirectory => _keysDirectory;
    
    /// <summary>
    /// Generates a new RSA-2048 key pair for the specified publisher.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <param name="algorithm">The key algorithm (currently only RSA-2048 is supported for generation).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PublisherKey"/> containing the generated key pair.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="publisherId"/> is null or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown if the requested algorithm is not supported.</exception>
    public async Task<PublisherKey> GenerateKeyPairAsync(string publisherId, KeyAlgorithm algorithm = KeyAlgorithm.Rsa2048, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(publisherId);
        
        if (algorithm == KeyAlgorithm.Rsa2048)
        {
            using var rsa = RSA.Create(2048);
            var publicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            var privateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey());
            
            var key = new PublisherKey
            {
                PublisherId = publisherId,
                Algorithm = algorithm,
                PublicKeyBase64 = publicKey,
                PrivateKeyBase64 = privateKey,
                CreatedAt = DateTime.UtcNow
            };
            
            await SaveKeyAsync(key, includePrivate: true, ct);
            return key;
        }
        
        throw new NotSupportedException("Only RSA-2048 is currently supported");
    }
    
    /// <summary>
    /// Retrieves the public key for a given publisher from disk.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <returns>The <see cref="PublisherKey"/> if found; otherwise <c>null</c>.</returns>
    public PublisherKey? GetPublicKey(string publisherId)
    {
        var path = GetKeyPath(publisherId, includePrivate: false);
        if (!File.Exists(path)) return null;
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PublisherKey>(json);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Saves a key to disk. Optionally includes the private key material.
    /// </summary>
    /// <param name="key">The key to save.</param>
    /// <param name="includePrivate">Whether to include the private key in the saved file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="key"/> is null.</exception>
    public async Task SaveKeyAsync(PublisherKey key, bool includePrivate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(key);
        var path = GetKeyPath(key.PublisherId, includePrivate);
        var toSave = includePrivate ? key : key with { PrivateKeyBase64 = null };
        var json = JsonSerializer.Serialize(toSave, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, ct);
    }
    
    /// <summary>
    /// Deletes the key file for a given publisher.
    /// </summary>
    /// <param name="publisherId">The publisher identifier.</param>
    /// <returns><c>true</c> if a key file was deleted; otherwise <c>false</c>.</returns>
    public bool DeleteKey(string publisherId)
    {
        var path = GetKeyPath(publisherId, includePrivate: true);
        if (File.Exists(path))
        {
            File.Delete(path);
            return true;
        }
        return false;
    }
    
    private string GetKeyPath(string publisherId, bool includePrivate)
    {
        var safe = string.Concat(publisherId.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_'));
        var suffix = includePrivate ? "private" : "public";
        return Path.Combine(_keysDirectory, $"{safe}.{suffix}.json");
    }
}
