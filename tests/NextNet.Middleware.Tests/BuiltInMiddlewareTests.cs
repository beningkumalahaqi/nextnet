using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextNet.Middleware.BuiltIn;
using NextNet.Middleware.Attributes;
using NextNet.Middleware.Extensions;
using Xunit;

namespace NextNet.Middleware.Tests;

public class BuiltInMiddlewareTests
{
    #region ErrorHandlingMiddleware

    [Fact]
    public async Task ErrorHandlingMiddleware_NoException_PassesThrough()
    {
        // Arrange
        var (middleware, ctx) = CreateErrorHandler();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => Task.CompletedTask);

        // Assert
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_Exception_CapturesAndReturns500()
    {
        // Arrange
        var (middleware, ctx) = CreateErrorHandler();
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => throw new InvalidOperationException("Test error"));

        // Assert
        Assert.Equal(500, ctx.HttpContext.Response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", ctx.HttpContext.Response.ContentType);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_OperationCanceled_Skips()
    {
        // Arrange
        var (middleware, ctx) = CreateErrorHandler();

        // Act - should not throw or set 500
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        ctx.HttpContext.RequestAborted = cts.Token;

        await middleware.InvokeAsync(ctx, next: _ => throw new OperationCanceledException(cts.Token));

        // Assert - response should not be modified since we canceled
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_CustomHandler_Invokes()
    {
        // Arrange
        var customInvoked = false;
        var options = new ErrorHandlingOptions
        {
            CustomErrorHandler = (ctx, ex) =>
            {
                customInvoked = true;
                return Task.CompletedTask;
            }
        };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));
        var sp = services.BuildServiceProvider();

        var middleware = new ErrorHandlingMiddleware(
            sp.GetRequiredService<ILogger<ErrorHandlingMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => throw new Exception("Test"));

        // Assert
        Assert.True(customInvoked);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_IncludeDetails_IncludesExceptionInfo()
    {
        // Arrange
        var options = new ErrorHandlingOptions { IncludeExceptionDetails = true };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var middleware = new ErrorHandlingMiddleware(
            sp.GetRequiredService<ILogger<ErrorHandlingMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => throw new InvalidOperationException("Test error"));

        // Assert
        ctx.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(ctx.HttpContext.Response.Body).ReadToEndAsync();
        Assert.Contains("Test error", body);
        Assert.Contains("InvalidOperationException", body);
    }

    [Fact]
    public async Task ErrorHandlingMiddleware_WithoutDetails_OmitsExceptionInfo()
    {
        // Arrange
        var options = new ErrorHandlingOptions { IncludeExceptionDetails = false };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var middleware = new ErrorHandlingMiddleware(
            sp.GetRequiredService<ILogger<ErrorHandlingMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => throw new InvalidOperationException("Test error"));

        // Assert
        ctx.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(ctx.HttpContext.Response.Body).ReadToEndAsync();
        Assert.DoesNotContain("Test error", body);
        Assert.Contains("Internal server error", body);
    }

    #endregion

    #region LoggingMiddleware

    [Fact]
    public async Task LoggingMiddleware_LogsRequestAndResponse()
    {
        // Arrange
        var (middleware, ctx) = CreateLoggingMiddleware();
        ctx.HttpContext.Request.Method = "GET";
        ctx.HttpContext.Request.Path = "/test";
        ctx.HttpContext.Response.StatusCode = 200;

        // Act - should not throw
        await middleware.InvokeAsync(ctx, next: _ => Task.CompletedTask);

        // Assert
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
    }

    [Fact]
    public async Task LoggingMiddleware_Exception_LogsWarning()
    {
        // Arrange
        var (middleware, ctx) = CreateLoggingMiddleware();
        ctx.HttpContext.Request.Method = "POST";
        ctx.HttpContext.Request.Path = "/error";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(ctx, next: _ => throw new InvalidOperationException("Boom")));

        Assert.Equal("Boom", ex.Message);
    }

    #endregion

    #region StaticFilesMiddleware

    [Fact]
    public async Task StaticFilesMiddleware_NonMatchingPath_PassesThrough()
    {
        // Arrange
        var (middleware, ctx) = CreateStaticFilesMiddleware();
        ctx.HttpContext.Request.Path = "/api/test";

        var downstreamInvoked = false;

        // Act
        await middleware.InvokeAsync(ctx, next: _ =>
        {
            downstreamInvoked = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(downstreamInvoked);
    }

    [Fact]
    public async Task StaticFilesMiddleware_NonExistentFile_PassesThrough()
    {
        // Arrange
        var (middleware, ctx) = CreateStaticFilesMiddleware();
        ctx.HttpContext.Request.Path = "/static/nonexistent.js";

        var downstreamInvoked = false;

        // Act
        await middleware.InvokeAsync(ctx, next: _ =>
        {
            downstreamInvoked = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(downstreamInvoked);
    }

    [Fact]
    public void StaticFilesOptions_DefaultsAreSane()
    {
        // Arrange
        var options = new StaticFilesOptions();

        // Assert
        Assert.Equal("/static", options.RequestPath);
        Assert.Equal(86400, options.CacheMaxAgeSeconds);
        Assert.False(options.ServeDefaultFiles);
    }

    #endregion

    #region CompressionMiddleware

    [Fact]
    public async Task CompressionMiddleware_NoAcceptEncoding_PassesThrough()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();

        var downstreamInvoked = false;

        // Act
        await middleware.InvokeAsync(ctx, next: _ =>
        {
            downstreamInvoked = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(downstreamInvoked);
    }

    [Fact]
    public async Task CompressionMiddleware_SmallResponse_NotCompressed()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware(minSize: 1000);
        ctx.HttpContext.Request.Headers.AcceptEncoding = "gzip";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var smallData = Encoding.UTF8.GetBytes("small");

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/plain";
            await httpCtx.Response.Body.WriteAsync(smallData);
        });

        // Assert - body should contain the original uncompressed data
        ctx.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var content = await new StreamReader(ctx.HttpContext.Response.Body).ReadToEndAsync();
        Assert.Equal("small", content);
        Assert.False(ctx.HttpContext.Response.Headers.ContainsKey("Content-Encoding"));
    }

    [Fact]
    public async Task CompressionMiddleware_GzipEncoding_CompressesResponse()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "gzip";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var largeData = Encoding.UTF8.GetBytes(new string('x', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/html";
            // Write to body before the middleware processes it
            await httpCtx.Response.Body.WriteAsync(largeData);
        });

        // Assert
        Assert.Equal("gzip", ctx.HttpContext.Response.Headers.ContentEncoding);
    }

    [Fact]
    public async Task CompressionMiddleware_BrotliEncoding_CompressesResponse()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "br";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var largeData = Encoding.UTF8.GetBytes(new string('y', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/html";
            await httpCtx.Response.Body.WriteAsync(largeData);
        });

        // Assert
        Assert.Equal("br", ctx.HttpContext.Response.Headers.ContentEncoding);
    }

    [Fact]
    public void CompressionOptions_DefaultsAreSane()
    {
        // Arrange
        var options = new CompressionOptions();

        // Assert
        Assert.Equal(256, options.MinimumSizeBytes);
        Assert.Contains("text/html", options.MimeTypes);
        Assert.Contains("application/json", options.MimeTypes);
    }

    #endregion

    #region Additional StaticFilesMiddleware Tests

    [Fact]
    public async Task StaticFilesMiddleware_ServesExistingFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);
        var testFilePath = Path.Combine(tempDir, "test.css");
        await File.WriteAllTextAsync(testFilePath, "body { color: red; }");

        var options = new StaticFilesOptions
        {
            RequestPath = "/static",
            ContentRootPath = tempDir
        };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Request.Path = "/static/test.css";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var downstreamInvoked = false;

        // Act
        await middleware.InvokeAsync(ctx, next: _ =>
        {
            downstreamInvoked = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.False(downstreamInvoked); // should not reach downstream
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
        Assert.Contains("text/css", ctx.HttpContext.Response.ContentType);
        Assert.True(ctx.HttpContext.Response.Headers.CacheControl.Count > 0);

        // Cleanup
        File.Delete(testFilePath);
    }

    [Fact]
    public async Task StaticFilesMiddleware_ServesWithCorrectContentType()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "app.js");
        await File.WriteAllTextAsync(testFile, "console.log('test');");

        var options = new StaticFilesOptions
        {
            RequestPath = "/static",
            ContentRootPath = tempDir
        };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Request.Path = "/static/app.js";
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => Task.CompletedTask);

        // Assert
        Assert.Contains("javascript", ctx.HttpContext.Response.ContentType);

        File.Delete(testFile);
    }

    [Fact]
    public async Task StaticFilesMiddleware_NoRequestPath_ServesAnyPath()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "file.txt");
        await File.WriteAllTextAsync(testFile, "hello");

        var options = new StaticFilesOptions
        {
            RequestPath = "",
            ContentRootPath = tempDir
        };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Request.Path = "/file.txt";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var downstreamInvoked = false;

        // Act
        await middleware.InvokeAsync(ctx, next: _ =>
        {
            downstreamInvoked = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.False(downstreamInvoked);
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);

        File.Delete(testFile);
    }

    [Fact]
    public async Task StaticFilesMiddleware_ServeDefaultFiles_ServesIndexHtml()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);
        var indexFile = Path.Combine(tempDir, "index.html");
        await File.WriteAllTextAsync(indexFile, "<html></html>");

        var options = new StaticFilesOptions
        {
            RequestPath = "/static",
            ContentRootPath = tempDir,
            ServeDefaultFiles = true
        };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Request.Path = "/static/";
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => Task.CompletedTask);

        // Assert
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
        Assert.Contains("text/html", ctx.HttpContext.Response.ContentType);

        File.Delete(indexFile);
    }

    [Fact]
    public async Task StaticFilesMiddleware_NonCacheableExtension_NoCacheHeader()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);
        var testFile = Path.Combine(tempDir, "doc.pdf");
        await File.WriteAllTextAsync(testFile, "%PDF-");

        var options = new StaticFilesOptions
        {
            RequestPath = "",
            ContentRootPath = tempDir
        };
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();
        ctx.HttpContext.Request.Path = "/doc.pdf";
        ctx.HttpContext.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(ctx, next: _ => Task.CompletedTask);

        // Assert
        Assert.Equal(200, ctx.HttpContext.Response.StatusCode);
        Assert.Contains("application/pdf", ctx.HttpContext.Response.ContentType);
        // PDF is not in the cacheable set
        Assert.False(ctx.HttpContext.Response.Headers.CacheControl.Count > 0);

        File.Delete(testFile);
    }

    #endregion

    #region Additional CompressionMiddleware Tests

    [Fact]
    public async Task CompressionMiddleware_DeflateEncoding_CompressesResponse()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "deflate";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var largeData = Encoding.UTF8.GetBytes(new string('z', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/html";
            await httpCtx.Response.Body.WriteAsync(largeData);
        });

        // Assert
        Assert.Equal("deflate", ctx.HttpContext.Response.Headers.ContentEncoding);
    }

    [Fact]
    public async Task CompressionMiddleware_NonCompressibleContentType_SkipsCompression()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "gzip";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var data = Encoding.UTF8.GetBytes(new string('a', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "image/png";
            await httpCtx.Response.Body.WriteAsync(data);
        });

        // Assert - should not set Content-Encoding for non-compressible types
        Assert.False(ctx.HttpContext.Response.Headers.ContainsKey("Content-Encoding"));
    }

    [Fact]
    public async Task CompressionMiddleware_IdentityEncoding_SkipsCompression()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "identity";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var data = Encoding.UTF8.GetBytes(new string('b', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/html";
            await httpCtx.Response.Body.WriteAsync(data);
        });

        // Assert
        Assert.False(ctx.HttpContext.Response.Headers.ContainsKey("Content-Encoding"));
    }

    [Fact]
    public async Task CompressionMiddleware_MultipleEncodings_PrefersPreferred()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        // Client sends gzip and deflate, should prefer gzip (our preferred)
        ctx.HttpContext.Request.Headers.AcceptEncoding = "deflate, gzip";
        ctx.HttpContext.Response.Body = new MemoryStream();

        var data = Encoding.UTF8.GetBytes(new string('c', 1000));

        // Act
        await middleware.InvokeAsync(ctx, next: async httpCtx =>
        {
            httpCtx.Response.ContentType = "text/html";
            await httpCtx.Response.Body.WriteAsync(data);
        });

        // Assert - should pick gzip since it's supported
        Assert.Equal("gzip", ctx.HttpContext.Response.Headers.ContentEncoding);
    }

    [Fact]
    public async Task CompressionMiddleware_Exception_RestoresOriginalBody()
    {
        // Arrange
        var (middleware, ctx) = CreateCompressionMiddleware();
        ctx.HttpContext.Request.Headers.AcceptEncoding = "gzip";
        // The default body is a NullStream from DefaultHttpContext
        var originalBody = ctx.HttpContext.Response.Body;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            middleware.InvokeAsync(ctx, next: _ => throw new InvalidOperationException("Boom")));

        // Assert - original body should be restored after exception
        Assert.Same(originalBody, ctx.HttpContext.Response.Body);
    }

    #endregion

    #region MiddlewareServiceCollectionExtensions Tests

    [Fact]
    public void AddNextNetMiddleware_NullServices_Throws()
    {
        ServiceCollection? nullServices = null;
        Assert.Throws<ArgumentNullException>(() =>
            nullServices!.AddNextNetMiddleware());
    }

    #endregion

    #region Attribute-Based Order Tests

    [Fact]
    public void LoggingMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(LoggingMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.Logging, attr.Order);
    }

    [Fact]
    public void StaticFilesMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(StaticFilesMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.StaticFiles, attr.Order);
    }

    [Fact]
    public void CompressionMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(CompressionMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.Compression, attr.Order);
    }

    [Fact]
    public void ErrorHandlingMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(ErrorHandlingMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.ErrorHandling, attr.Order);
    }

    [Fact]
    public void CorsMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(CorsMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.Early, attr.Order);
    }

    [Fact]
    public void SecurityHeadersMiddleware_HasCorrectAttributeOrder()
    {
        var attr = (MiddlewareOrderAttribute?)Attribute.GetCustomAttribute(
            typeof(SecurityHeadersMiddleware), typeof(MiddlewareOrderAttribute));
        Assert.NotNull(attr);
        Assert.Equal(MiddlewareOrder.Early + 10, attr.Order);
    }

    #endregion

    #region Helpers

    private static (ErrorHandlingMiddleware Middleware, MiddlewareContext Context) CreateErrorHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var middleware = new ErrorHandlingMiddleware(
            sp.GetRequiredService<ILogger<ErrorHandlingMiddleware>>());
        var ctx = CreateMiddlewareContext();

        return (middleware, ctx);
    }

    private static (LoggingMiddleware Middleware, MiddlewareContext Context) CreateLoggingMiddleware()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var middleware = new LoggingMiddleware(
            sp.GetRequiredService<ILogger<LoggingMiddleware>>());
        var ctx = CreateMiddlewareContext();

        return (middleware, ctx);
    }

    private static (StaticFilesMiddleware Middleware, MiddlewareContext Context) CreateStaticFilesMiddleware()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        // Use a temp directory for static files to avoid DirectoryNotFoundException
        var tempDir = Path.Combine(Path.GetTempPath(), "NextNetTests", "wwwroot");
        Directory.CreateDirectory(tempDir);

        var options = new StaticFilesOptions
        {
            RequestPath = "/static",
            ContentRootPath = tempDir
        };
        var middleware = new StaticFilesMiddleware(
            sp.GetRequiredService<ILogger<StaticFilesMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();

        return (middleware, ctx);
    }

    private static (CompressionMiddleware Middleware, MiddlewareContext Context) CreateCompressionMiddleware(int minSize = 256)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var options = new CompressionOptions { MinimumSizeBytes = minSize };
        var middleware = new CompressionMiddleware(
            sp.GetRequiredService<ILogger<CompressionMiddleware>>(),
            Microsoft.Extensions.Options.Options.Create(options));
        var ctx = CreateMiddlewareContext();

        return (middleware, ctx);
    }

    private static MiddlewareContext CreateMiddlewareContext()
    {
        var httpContext = new DefaultHttpContext();
        var pipeline = new MiddlewarePipeline();
        return new MiddlewareContext(httpContext, pipeline);
    }

    #endregion
}
