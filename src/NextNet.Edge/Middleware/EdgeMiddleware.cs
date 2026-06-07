using Microsoft.AspNetCore.Http;
using NextNet.Edge.Adapters;
using NextNet.Edge.Compatibility;
using NextNet.Rendering;
using NextNet.Routing;

namespace NextNet.Edge.Middleware;

/// <summary>
/// ASP.NET Core middleware that enables edge runtime simulation.
/// When running in standard ASP.NET Core, this middleware adapts requests/responses
/// to behave as they would on edge, flagging any incompatibilities.
/// </summary>
public class EdgeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly EdgeOptions _options;
    private readonly EdgeCompatibilityChecker _compatibilityChecker;
    private readonly AdapterRegistry _adapterRegistry;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="options">Edge configuration options.</param>
    /// <param name="compatibilityChecker">The compatibility checker for edge API validation.</param>
    /// <param name="adapterRegistry">The adapter registry for resolving edge providers.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public EdgeMiddleware(
        RequestDelegate next,
        EdgeOptions options,
        EdgeCompatibilityChecker compatibilityChecker,
        AdapterRegistry adapterRegistry)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compatibilityChecker = compatibilityChecker ?? throw new ArgumentNullException(nameof(compatibilityChecker));
        _adapterRegistry = adapterRegistry ?? throw new ArgumentNullException(nameof(adapterRegistry));
    }

    /// <summary>
    /// Invokes the edge middleware for the current request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        // In edge simulation mode, add edge-specific headers and constraints
        if (!context.Response.Headers.ContainsKey("x-edge-provider"))
        {
            context.Response.Headers["x-edge-provider"] = _options.Provider;
            context.Response.Headers["x-edge-simulated"] = "true";
        }

        // Set a size limit on the response to simulate edge budget constraints
        if (_options.MaxBundleSize > 0)
        {
            var originalBody = context.Response.Body;
            var budgetStream = new EdgeBudgetStream(originalBody, _options.MaxBundleSize);
            context.Response.Body = budgetStream;
        }

        await _next(context);
    }
}

/// <summary>
/// A write-only stream that enforces a size budget. Throws if the budget is exceeded.
/// Used by <see cref="EdgeMiddleware"/> to simulate edge deployment size constraints.
/// </summary>
internal class EdgeBudgetStream : Stream
{
    private readonly Stream _innerStream;
    private readonly long _maxSize;
    private long _bytesWritten;

    public EdgeBudgetStream(Stream innerStream, long maxSize)
    {
        _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        _maxSize = maxSize;
        _bytesWritten = 0;
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _bytesWritten;
    public override long Position
    {
        get => _bytesWritten;
        set => throw new NotSupportedException("EdgeBudgetStream does not support seeking.");
    }

    public override void Flush() => _innerStream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) =>
        _innerStream.FlushAsync(cancellationToken);

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException("EdgeBudgetStream is write-only.");

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException("EdgeBudgetStream does not support seeking.");

    public override void SetLength(long value) =>
        throw new NotSupportedException("EdgeBudgetStream does not support setting length.");

    public override void Write(byte[] buffer, int offset, int count)
    {
        CheckBudget(count);
        _innerStream.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        CheckBudget(count);
        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        CheckBudget(buffer.Length);
        await _innerStream.WriteAsync(buffer, cancellationToken);
    }

    private void CheckBudget(int count)
    {
        if (_bytesWritten + count > _maxSize)
        {
            throw new InvalidOperationException(
                $"Edge size budget exceeded: {_bytesWritten + count} bytes written " +
                $"but maximum is {_maxSize} bytes ({_maxSize / 1024.0:F1} KB). " +
                "Reduce bundle size or increase EdgeOptions.MaxBundleSize.");
        }
        Interlocked.Add(ref _bytesWritten, count);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _innerStream.Dispose();
        base.Dispose(disposing);
    }
}
