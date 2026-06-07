using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using NextNet.Data.Sdk.Analyzers;

namespace NextNet.Data.Sdk.Tests.Analyzers;

/// <summary>
/// Integration-style tests for <see cref="ProviderAnalyzer"/> with real attribute types from the SDK assembly.
/// </summary>
public class ProviderAnalyzerIntegrationTests
{
    /// <summary>
    /// Verifies that a properly attributed and sealed DataProvider produces no diagnostics.
    /// </summary>
    [Fact]
    public async Task WellFormedDataProvider_Should_ProduceNoDiagnostics()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Sdk;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;

[DataProvider(Name = ""Test"", Description = ""Test provider"")]
public sealed class GoodProvider : IDataProvider
{
    public string Name => ""Test"";
    public Task InitializeAsync(DataConfig config, CancellationToken ct = default) => Task.CompletedTask;
    public Task<HealthCheckResult> IsHealthyAsync(CancellationToken ct = default)
        => Task.FromResult(new HealthCheckResult(true, ""Healthy"", TimeSpan.Zero));
}
";

        await RunIntegrationTest(code);
    }

    /// <summary>
    /// Verifies that a DataProvider without the attribute and not sealed triggers SKDataSdk001.
    /// </summary>
    [Fact]
    public async Task UnattributedUnsealedDataProvider_Should_ProduceSKDataSdk001()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Sdk;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;

public class BadProvider : IDataProvider
{
    public string Name => ""Bad"";
    public Task InitializeAsync(DataConfig config, CancellationToken ct = default) => Task.CompletedTask;
    public Task<HealthCheckResult> IsHealthyAsync(CancellationToken ct = default)
        => Task.FromResult(new HealthCheckResult(true, ""Healthy"", TimeSpan.Zero));
}
";

        var expected = DiagnosticResult
            .CompilerError("SKDataSdk001")
            .WithSpan(10, 14, 10, 25)
            .WithArguments("BadProvider");

        await RunIntegrationTest(code, expected);
    }

    /// <summary>
    /// Verifies that a MigrationEngine in a wrong namespace triggers SKDataSdk003.
    /// </summary>
    [Fact]
    public async Task MigrationEngineInWrongNamespace_Should_ProduceSKDataSdk003()
    {
        var code = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Sdk;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Models;

namespace WrongNamespace
{
    [DataMigrationEngine]
    public class MyMigration : IMigrationEngine
    {
        public Task<MigrationResult> AddMigrationAsync(string name, CancellationToken ct = default)
            => Task.FromResult(new MigrationResult(true, """"));
        public Task<MigrationResult> ApplyAsync(CancellationToken ct = default)
            => Task.FromResult(new MigrationResult(true, """"));
        public Task<MigrationResult> RollbackAsync(CancellationToken ct = default)
            => Task.FromResult(new MigrationResult(true, """"));
    }
}
";

        var expected = DiagnosticResult
            .CompilerWarning("SKDataSdk003")
            .WithSpan(12, 18, 12, 29)
            .WithArguments("MyMigration", "WrongNamespace");

        await RunIntegrationTest(code, expected);
    }

    private static async Task RunIntegrationTest(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<ProviderAnalyzer, DefaultVerifier>
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80,
            CompilerDiagnostics = CompilerDiagnostics.Errors,
        };

        test.TestState.AdditionalReferences.Add(
            typeof(NextNet.Data.Sdk.DataProviderAttribute).Assembly);

        test.TestState.AdditionalReferences.Add(
            typeof(NextNet.Data.Abstractions.Abstractions.IDataProvider).Assembly);

        foreach (var diag in expected)
        {
            test.ExpectedDiagnostics.Add(diag);
        }

        await test.RunAsync(CancellationToken.None);
    }
}
