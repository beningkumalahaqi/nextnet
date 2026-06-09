namespace NextNet.TemplateSecurity;

using System.Security.Cryptography;

/// <summary>
/// Verifies and creates cryptographic signatures for template packages.
/// </summary>
public sealed class SignatureVerifier
{
    /// <summary>
    /// Verifies a signature against data using the publisher's public key.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="publicKey">The publisher's public key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public bool Verify(byte[] data, byte[] signature, PublisherKey publicKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(publicKey);
        
        try
        {
            var keyBytes = Convert.FromBase64String(publicKey.PublicKeyBase64);
            
            return publicKey.Algorithm switch
            {
                KeyAlgorithm.Rsa2048 => VerifyRsa(data, signature, keyBytes),
                KeyAlgorithm.Ed25519 => VerifyEd25519(data, signature, keyBytes),
                _ => throw new NotSupportedException($"Algorithm {publicKey.Algorithm} not supported")
            };
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Signs data with the publisher's private key.
    /// </summary>
    /// <param name="data">The data to sign.</param>
    /// <param name="privateKey">The publisher's key containing the private key material.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The signature bytes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the private key is not available.</exception>
    /// <exception cref="NotSupportedException">Thrown if the algorithm is not supported for signing.</exception>
    public byte[] Sign(byte[] data, PublisherKey privateKey, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(privateKey);
        
        if (string.IsNullOrEmpty(privateKey.PrivateKeyBase64))
            throw new InvalidOperationException("Private key not available for signing.");
        
        var keyBytes = Convert.FromBase64String(privateKey.PrivateKeyBase64);
        
        return privateKey.Algorithm switch
        {
            KeyAlgorithm.Rsa2048 => SignRsa(data, keyBytes),
            KeyAlgorithm.Ed25519 => throw new NotSupportedException("Ed25519 signing requires platform-specific support"),
            _ => throw new NotSupportedException($"Algorithm {privateKey.Algorithm} not supported")
        };
    }
    
    private static bool VerifyRsa(byte[] data, byte[] signature, byte[] keyBytes)
    {
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
    
    private static bool VerifyEd25519(byte[] data, byte[] signature, byte[] keyBytes)
    {
        throw new NotSupportedException("Ed25519 is not yet supported. Use RSA instead.");
    }
    
    private static byte[] SignRsa(byte[] data, byte[] keyBytes)
    {
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
