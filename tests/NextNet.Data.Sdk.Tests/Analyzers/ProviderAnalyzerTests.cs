using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NextNet.Data.Sdk.Analyzers;

namespace NextNet.Data.Sdk.Tests.Analyzers;

/// <summary>
/// Tests for <see cref="ProviderAnalyzer"/>.
/// </summary>
public class ProviderAnalyzerTests
{
    // Interfaces from NextNet.Data.Abstractions are defined in-line for test
    // compilation since the test project references the actual package.

    [Fact]
    public async Task SKDataSdk001_Should_Report_When_DataProviderNotSealedAndNoAttribute()
    {
        var code = @"
using System.Threading;
using System.Threading.Tasks;

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IDataProvider
    {
        string Name { get; }
        Task InitializeAsync(object config, CancellationToken ct = default);
        Task<object> IsHealthyAsync(CancellationToken ct = default);
    }

    public interface IHealthCheckProvider
    {
        Task<object> GetHealthCheckAsync(CancellationToken ct = default);
    }
}

namespace Test
{
    using NextNet.Data.Abstractions.Abstractions;

    public class MyDataProvider : IDataProvider
    {
        public string Name => ""My"";
        public Task InitializeAsync(object config, CancellationToken ct = default) => Task.CompletedTask;
        public Task<object> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
    }
}
";
        var expected = DiagnosticResult
            .CompilerError("SKDataSdk001")
            .WithSpan(24, 18, 24, 32) // MyDataProvider class
            .WithArguments("MyDataProvider");

        await RunAnalyzerTest(code, expected);
    }

    [Fact]
    public async Task SKDataSdk001_Should_NotReport_When_DataProviderIsSealed()
    {
        var code = @"
using System.Threading;
using System.Threading.Tasks;

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IDataProvider
    {
        string Name { get; }
        Task InitializeAsync(object config, CancellationToken ct = default);
        Task<object> IsHealthyAsync(CancellationToken ct = default);
    }
}

namespace Test
{
    using NextNet.Data.Abstractions.Abstractions;

    public sealed class MyDataProvider : IDataProvider
    {
        public string Name => ""My"";
        public Task InitializeAsync(object config, CancellationToken ct = default) => Task.CompletedTask;
        public Task<object> IsHealthyAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
    }
}
";

        await RunAnalyzerTest(code);
    }

    [Fact]
    public async Task SKDataSdk002_Should_Report_When_RepositoryMissingAttribute()
    {
        var code = @"
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IRepository<T> where T : class
    {
        Task<T?> FindAsync(object id, CancellationToken ct = default);
        Task<object> GetAllAsync(object? options = null, CancellationToken ct = default);
        Task InsertAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(object id, CancellationToken ct = default);
    }
}

namespace Test
{
    using NextNet.Data.Abstractions.Abstractions;

    public class MyRepository : IRepository<object>
    {
        public Task<object?> FindAsync(object id, CancellationToken ct = default) => Task.FromResult<object?>(null);
        public Task<object> GetAllAsync(object? options = null, CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task InsertAsync(object entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(object entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(object id, CancellationToken ct = default) => Task.CompletedTask;
    }
}
";
        var expected = DiagnosticResult
            .CompilerError("SKDataSdk002")
            .WithSpan(22, 18, 22, 30) // MyRepository class
            .WithArguments("MyRepository");

        await RunAnalyzerTest(code, expected);
    }

    [Fact]
    public async Task SKDataSdk002_Should_NotReport_When_RepositoryHasAttribute()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NextNet.Data.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataRepositoryAttribute<T> : Attribute where T : class { }
}

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IRepository<T> where T : class
    {
        Task<T?> FindAsync(object id, CancellationToken ct = default);
        Task<object> GetAllAsync(object? options = null, CancellationToken ct = default);
        Task InsertAsync(T entity, CancellationToken ct = default);
        Task UpdateAsync(T entity, CancellationToken ct = default);
        Task DeleteAsync(object id, CancellationToken ct = default);
    }
}

namespace Test
{
    using NextNet.Data.Sdk;
    using NextNet.Data.Abstractions.Abstractions;

    [DataRepository<object>]
    public class MyRepository : IRepository<object>
    {
        public Task<object?> FindAsync(object id, CancellationToken ct = default) => Task.FromResult<object?>(null);
        public Task<object> GetAllAsync(object? options = null, CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task InsertAsync(object entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(object entity, CancellationToken ct = default) => Task.CompletedTask;
        public Task DeleteAsync(object id, CancellationToken ct = default) => Task.CompletedTask;
    }
}
";

        await RunAnalyzerTest(code);
    }

    [Fact]
    public async Task SKDataSdk003_Should_Report_When_MigrationEngineInWrongNamespace()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NextNet.Data.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataMigrationEngineAttribute : Attribute { }
}

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IMigrationEngine
    {
        Task<object> AddMigrationAsync(string name, CancellationToken ct = default);
        Task<object> ApplyAsync(CancellationToken ct = default);
        Task<object> RollbackAsync(CancellationToken ct = default);
    }
}

namespace Test.Services
{
    using NextNet.Data.Sdk;
    using NextNet.Data.Abstractions.Abstractions;

    [DataMigrationEngine]
    public class MyMigrationEngine : IMigrationEngine
    {
        public Task<object> AddMigrationAsync(string name, CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task<object> ApplyAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task<object> RollbackAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
    }
}
";
        var expected = DiagnosticResult
            .CompilerWarning("SKDataSdk003")
            .WithSpan(28, 18, 28, 35) // MyMigrationEngine class
            .WithArguments("MyMigrationEngine", "Test.Services");

        await RunAnalyzerTest(code, expected);
    }

    [Fact]
    public async Task SKDataSdk003_Should_NotReport_When_MigrationEngineInCorrectNamespace()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NextNet.Data.Sdk
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataMigrationEngineAttribute : Attribute { }
}

namespace NextNet.Data.Abstractions.Abstractions
{
    public interface IMigrationEngine
    {
        Task<object> AddMigrationAsync(string name, CancellationToken ct = default);
        Task<object> ApplyAsync(CancellationToken ct = default);
        Task<object> RollbackAsync(CancellationToken ct = default);
    }
}

namespace Test.Migrations
{
    using NextNet.Data.Sdk;
    using NextNet.Data.Abstractions.Abstractions;

    [DataMigrationEngine]
    public class MyMigrationEngine : IMigrationEngine
    {
        public Task<object> AddMigrationAsync(string name, CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task<object> ApplyAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
        public Task<object> RollbackAsync(CancellationToken ct = default) => Task.FromResult<object>(null!);
    }
}
";

        await RunAnalyzerTest(code);
    }

    /// <summary>
    /// Runs the <see cref="ProviderAnalyzer"/> against the given source code and verifies diagnostics.
    /// </summary>
    private static async Task RunAnalyzerTest(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ProviderAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.Errors,
        };

        // Note: AnalyzerConfigFiles (.editorconfig) is not supported by
        // Microsoft.CodeAnalysis.Testing version 1.1.2. The severity for each
        // diagnostic is already set via DiagnosticResult.CompilerError/CompilerWarning
        // and matches the defaults in DiagnosticDescriptors.

        // Add the SDK assembly as a reference so our attributes are available
        test.TestState.AdditionalReferences.Add(
            typeof(NextNet.Data.Sdk.DataProviderAttribute).Assembly);

        // Add the Abstractions assembly as a reference so interfaces are available
        test.TestState.AdditionalReferences.Add(
            typeof(NextNet.Data.Abstractions.Abstractions.IDataProvider).Assembly);

        foreach (var diag in expected)
        {
            test.ExpectedDiagnostics.Add(diag);
        }

        await test.RunAsync(CancellationToken.None);
    }
}
