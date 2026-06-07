namespace NextNet.TemplateSecurity;

/// <summary>
/// Orchestrates template security validation including checksum verification,
/// signature verification, and trusted publisher checks.
/// </summary>
public sealed class TemplateSecurityValidator
{
    private readonly ChecksumVerifier _checksumVerifier;
    private readonly SignatureVerifier _signatureVerifier;
    private readonly TrustedPublisherRegistry _publisherRegistry;
    private readonly SecurityAuditLogger _auditLogger;
    private readonly SecurityOptions _options;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateSecurityValidator"/> class.
    /// </summary>
    /// <param name="checksumVerifier">The checksum verifier service.</param>
    /// <param name="signatureVerifier">The signature verifier service.</param>
    /// <param name="publisherRegistry">The trusted publisher registry.</param>
    /// <param name="auditLogger">The security audit logger.</param>
    /// <param name="options">Security configuration options.</param>
    public TemplateSecurityValidator(
        ChecksumVerifier checksumVerifier,
        SignatureVerifier signatureVerifier,
        TrustedPublisherRegistry publisherRegistry,
        SecurityAuditLogger auditLogger,
        SecurityOptions options)
    {
        _checksumVerifier = checksumVerifier;
        _signatureVerifier = signatureVerifier;
        _publisherRegistry = publisherRegistry;
        _auditLogger = auditLogger;
        _options = options;
    }
    
    /// <summary>
    /// Validates a template package stream against the provided security context.
    /// Performs checksum verification, signature verification, and publisher trust checks
    /// based on the configured security level.
    /// </summary>
    /// <param name="packageStream">The stream containing the package content.</param>
    /// <param name="context">The security context with expected checksums, signatures, and publisher info.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="SecurityReport"/> detailing the results of all checks.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="packageStream"/> or <paramref name="context"/> is null.</exception>
    public async Task<SecurityReport> ValidateAsync(
        Stream packageStream,
        SecurityContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageStream);
        ArgumentNullException.ThrowIfNull(context);
        
        var warnings = new List<string>();
        var errors = new List<string>();
        var checksumValid = false;
        var signatureValid = false;
        var publisherTrusted = false;
        var publisherRevoked = false;
        
        // Buffer the stream so we can perform multiple operations
        var buffer = await BufferStreamAsync(packageStream, cancellationToken);
        
        try
        {
            // 1. Checksum verification (always required unless disabled)
            if (_options.RequireChecksums && !string.IsNullOrEmpty(context.ExpectedChecksum))
            {
                buffer.Position = 0;
                checksumValid = await _checksumVerifier.VerifyAsync(buffer, context.ExpectedChecksum, cancellationToken);
                if (!checksumValid)
                {
                    errors.Add("Checksum verification failed");
                    await _auditLogger.LogEventAsync("CHECKSUM_FAILED",
                        $"Package {context.PackageName} v{context.PackageVersion} failed checksum check",
                        new Dictionary<string, string> { ["publisher"] = context.PublisherId },
                        cancellationToken);
                }
            }
            else if (string.IsNullOrEmpty(context.ExpectedChecksum))
            {
                warnings.Add("No checksum provided - cannot verify package integrity");
            }
            else
            {
                checksumValid = true; // Skipped by configuration
            }
            
            // 2. Signature verification
            if (context.Signature is { Length: > 0 } && context.PublisherKey is not null)
            {
                buffer.Position = 0;
                signatureValid = _signatureVerifier.Verify(
                    await ReadAllBytesAsync(buffer, cancellationToken),
                    context.Signature,
                    context.PublisherKey,
                    cancellationToken);
                
                if (!signatureValid)
                {
                    errors.Add("Signature verification failed");
                    await _auditLogger.LogEventAsync("SIGNATURE_FAILED",
                        $"Package {context.PackageName} failed signature check",
                        null,
                        cancellationToken);
                }
            }
            else if (context.SecurityLevel == SecurityLevel.Strict)
            {
                errors.Add("Strict mode requires signature");
            }
            else
            {
                warnings.Add("No signature provided");
            }
            
            // 3. Publisher trust check
            if (!string.IsNullOrEmpty(context.PublisherId))
            {
                publisherRevoked = _publisherRegistry.IsRevoked(context.PublisherId);
                if (publisherRevoked)
                {
                    errors.Add($"Publisher '{context.PublisherId}' has been revoked");
                    await _auditLogger.LogEventAsync("REVOKED_PUBLISHER",
                        $"Attempted install from revoked publisher: {context.PublisherId}",
                        null,
                        cancellationToken);
                }
                
                publisherTrusted = _publisherRegistry.IsTrusted(context.PublisherId);
                if (_options.EnforceTrustedPublishers && !publisherTrusted)
                {
                    errors.Add($"Publisher '{context.PublisherId}' is not in the trusted list");
                }
            }
            
            var isValid = errors.Count == 0 && checksumValid && !publisherRevoked;
            
            return new SecurityReport
            {
                IsValid = isValid,
                ChecksumValid = checksumValid,
                SignatureValid = signatureValid,
                PublisherTrusted = publisherTrusted,
                PublisherRevoked = publisherRevoked,
                Warnings = warnings,
                Errors = errors
            };
        }
        finally
        {
            buffer.Dispose();
        }
    }
    
    private static async Task<MemoryStream> BufferStreamAsync(Stream source, CancellationToken ct)
    {
        var ms = new MemoryStream();
        await source.CopyToAsync(ms, ct);
        ms.Position = 0;
        return ms;
    }
    
    private static async Task<byte[]> ReadAllBytesAsync(Stream stream, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        return ms.ToArray();
    }
}
