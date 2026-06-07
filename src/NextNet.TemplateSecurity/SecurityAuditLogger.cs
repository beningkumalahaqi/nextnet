namespace NextNet.TemplateSecurity;

/// <summary>
/// Logs security audit events to a JSON Lines file for forensic analysis.
/// Each line is a JSON object with timestamp, event type, message, and optional data.
/// </summary>
public sealed class SecurityAuditLogger
{
    private readonly SecurityOptions _options;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityAuditLogger"/> class.
    /// </summary>
    /// <param name="options">Security options containing the audit log file path.</param>
    public SecurityAuditLogger(SecurityOptions options) => _options = options;
    
    /// <summary>
    /// Logs a security audit event as a JSON line to the audit log file.
    /// </summary>
    /// <param name="eventType">The type/category of the event (e.g., "CHECKSUM_FAILED", "SIGNATURE_FAILED").</param>
    /// <param name="message">A human-readable description of the event.</param>
    /// <param name="data">Optional key-value pairs with additional context.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task LogEventAsync(string eventType, string message, IReadOnlyDictionary<string, string>? data = null, CancellationToken ct = default)
    {
        var dir = Path.GetDirectoryName(_options.AuditLogFile)!;
        Directory.CreateDirectory(dir);
        
        var entry = new
        {
            timestamp = DateTime.UtcNow.ToString("O"),
            eventType,
            message,
            data = data ?? new Dictionary<string, string>()
        };
        
        var json = System.Text.Json.JsonSerializer.Serialize(entry);
        await File.AppendAllTextAsync(_options.AuditLogFile, json + Environment.NewLine, ct);
    }
}
