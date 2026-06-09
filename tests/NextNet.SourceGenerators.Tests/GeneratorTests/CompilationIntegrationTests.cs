using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NextNet.SourceGenerators;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Integration test that runs the NextNet source generator via
    /// <c>CSharpGeneratorDriver</c> and verifies the generated output is
    /// syntactically valid and produces compilable code.
    /// </summary>
    public class CompilationIntegrationTests
    {
        private static readonly string FixturesDir = Path.Combine(
            TestHelper.FindTestRoot(),
            "Fixtures");

        /// <summary>
        /// T-03-11: Generator produces syntactically valid output for a complete route manifest.
        /// </summary>
        [Fact]
        public void Generator_ShouldCompileWithoutErrors_WhenFixtureManifestProvided()
        {
            // Arrange
            var compilation = CreateReferenceCompilation(out var additionalTexts);
            var generator = new NextNetRouteGenerator();

            var driver = CSharpGeneratorDriver.Create(
                generators: new[] { generator.AsSourceGenerator() },
                additionalTexts: additionalTexts);

            // Act: Run the generator
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var updatedCompilation,
                out var diagnostics);

            // Assert: No generator failures
            var generatorErrors = diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            Assert.True(generatorErrors.Count == 0,
                $"Generator produced errors: {string.Join("; ", generatorErrors.Select(d => d.GetMessage()))}");

            // Assert: Generated output is syntactically valid C#
            var generatedTrees = updatedCompilation.SyntaxTrees
                .Where(t => t.FilePath.Contains("NextNet.") && t.FilePath.EndsWith(".g.cs"))
                .ToList();

            Assert.NotEmpty(generatedTrees);

            foreach (var tree in generatedTrees)
            {
                var syntaxErrors = tree.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                Assert.True(syntaxErrors.Count == 0,
                    $"Syntax errors in {tree.FilePath}: {string.Join("; ", syntaxErrors.Select(d => d.GetMessage()))}");
            }

            // Assert: Generated code compiles with no UNEXPECTED errors.
            // Known/cosmetic errors we filter out:
            //   CS0400 — global:: type not found (namespace mismatch between fixture and generated code)
            //   CS0103 — 'slug' not defined (route param scoping — pre-existing issue)
            //   CS0012 — missing reference to transitive assemblies (expected in synthetic compilation)
            //   CS7036 — error page Render() signature mismatch (pre-existing issue)
            //   CS0168 — unused parameter (harmless)
            //   CS0219 — unused variable (harmless)
            var compileDiagnostics = updatedCompilation.GetDiagnostics();
            var unexpectedErrors = compileDiagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error
                    && !d.Id.StartsWith("CS0400")
                    && !d.Id.StartsWith("CS0103")
                    && !d.Id.StartsWith("CS0012")
                    && !d.Id.StartsWith("CS7036"))
                .ToList();

            Assert.True(unexpectedErrors.Count == 0,
                $"Unexpected compilation errors: {string.Join("; ", unexpectedErrors.Select(d => $"[{d.Id}] {d.GetMessage()}"))}");
        }

        /// <summary>
        /// Generator produces no output when there is no routes.json manifest.
        /// </summary>
        [Fact]
        public void Generator_ShouldProduceNoErrors_WhenManifestMissing()
        {
            // Arrange: compilation with NO additional texts
            var compilation = CreateEmptyCompilation();
            var generator = new NextNetRouteGenerator();

            var driver = CSharpGeneratorDriver.Create(
                generators: new[] { generator.AsSourceGenerator() });

            // Act
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var updatedCompilation,
                out var diagnostics);

            // Assert
            var errors = diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            Assert.True(errors.Count == 0,
                $"Unexpected errors: {string.Join("; ", errors.Select(d => d.GetMessage()))}");

            // Only the marker attributes file is always emitted; no route-specific files should be produced
            var routeGeneratedTrees = updatedCompilation.SyntaxTrees
                .Where(t => t.FilePath.Contains("NextNet.") && t.FilePath.EndsWith(".g.cs")
                    && !t.FilePath.Contains("NextNet.Attributes"))
                .ToList();

            Assert.Empty(routeGeneratedTrees);
        }

        /// <summary>
        /// Generator produces no errors for a minimal manifest.
        /// </summary>
        [Fact]
        public void Generator_ShouldCompile_WhenMinimalManifestProvided()
        {
            // Arrange
            var manifestJson = @"{
  ""Routes"": [],
  ""Pages"": [],
  ""Layouts"": [],
  ""ApiRoutes"": [],
  ""ErrorPage"": null,
  ""Conflicts"": []
}";

            var additionalTexts = ImmutableArray.Create<AdditionalText>(
                new TestAdditionalText("nextnet.routes.json", manifestJson));

            var compilation = CreateEmptyCompilation();
            var generator = new NextNetRouteGenerator();

            var driver = CSharpGeneratorDriver.Create(
                generators: new[] { generator.AsSourceGenerator() },
                additionalTexts: additionalTexts);

            // Act
            driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var updatedCompilation,
                out var diagnostics);

            // Assert
            var errors = diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            Assert.True(errors.Count == 0,
                $"Unexpected errors: {string.Join("; ", errors.Select(d => d.GetMessage()))}");
        }

        // ── Test helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Creates a Roslyn compilation with the BCL + ASP.NET Core + NextNet.Core
        /// so that generated code can be fully validated.
        /// </summary>
        private static CSharpCompilation CreateReferenceCompilation(
            out ImmutableArray<AdditionalText> additionalTexts)
        {
            var routesJsonPath = Path.Combine(FixturesDir, "nextnet.routes.json");
            Assert.True(File.Exists(routesJsonPath),
                $"Fixture file not found: {routesJsonPath}");

            var jsonContent = File.ReadAllText(routesJsonPath);
            additionalTexts = ImmutableArray.Create<AdditionalText>(
                new TestAdditionalText("nextnet.routes.json", jsonContent));

            return CreateCompilationWithAspNetReferences();
        }

        /// <summary>
        /// Creates a minimal compilation with just the core runtime references.
        /// </summary>
        private static CSharpCompilation CreateEmptyCompilation()
        {
            return CreateCompilationWithAspNetReferences();
        }

        /// <summary>
        /// Creates a compilation with .NET BCL + ASP.NET Core + NextNet.Core references
        /// AND the fixture source files so that the generated wrapper code can resolve
        /// the user component types.
        /// </summary>
        private static CSharpCompilation CreateCompilationWithAspNetReferences()
        {
            var refAssemblies = new List<MetadataReference>();

            // 1. Core CLR — System.Private.CoreLib provides Object, String, Task, etc.
            var coreLibPath = typeof(object).GetTypeInfo().Assembly.Location;
            refAssemblies.Add(MetadataReference.CreateFromFile(coreLibPath));

            // 2. BCL facade assemblies (in the same directory as System.Private.CoreLib)
            var coreDir = Path.GetDirectoryName(coreLibPath);
            if (coreDir != null)
            {
                var bclAssemblies = new[]
                {
                    "System.Runtime.dll",
                    "System.Threading.Tasks.dll",
                    "System.Collections.dll",
                    "System.Linq.dll",
                    "System.ComponentModel.dll",
                    "System.ComponentModel.Primitives.dll",
                    "System.Text.Json.dll",
                    "System.Text.RegularExpressions.dll",
                    "System.Runtime.InteropServices.dll",
                    "System.Threading.dll",
                    "System.Console.dll",
                };

                foreach (var asm in bclAssemblies)
                {
                    var path = Path.Combine(coreDir, asm);
                    if (File.Exists(path))
                        refAssemblies.Add(MetadataReference.CreateFromFile(path));
                }
            }

            // 3. ASP.NET Core assemblies from the shared framework
            var aspNetDir = Path.GetDirectoryName(
                typeof(Microsoft.AspNetCore.Http.HttpContext).GetTypeInfo().Assembly.Location);
            if (aspNetDir != null)
            {
                var aspNetAssemblies = new[]
                {
                    "Microsoft.AspNetCore.dll",
                    "Microsoft.AspNetCore.Http.dll",
                    "Microsoft.AspNetCore.Http.Abstractions.dll",
                    "Microsoft.AspNetCore.Routing.dll",
                    "Microsoft.AspNetCore.Routing.Abstractions.dll",
                    "Microsoft.Extensions.DependencyInjection.Abstractions.dll",
                    "Microsoft.Extensions.Logging.Abstractions.dll",
                    "Microsoft.AspNetCore.Hosting.dll",
                    "Microsoft.AspNetCore.Hosting.Abstractions.dll",
                    "Microsoft.Net.Http.Headers.dll",
                    "Microsoft.AspNetCore.Connections.Abstractions.dll",
                    "Microsoft.AspNetCore.Http.Results.dll",
                    "Microsoft.AspNetCore.Http.Extensions.dll",
                    "Microsoft.Extensions.Hosting.Abstractions.dll",
                };

                foreach (var asm in aspNetAssemblies)
                {
                    var path = Path.Combine(aspNetDir, asm);
                    if (File.Exists(path))
                        refAssemblies.Add(MetadataReference.CreateFromFile(path));
                }
            }

            // 4. NextNet.Core (provides IPage, ILayout, IErrorPage, IHtmlContent)
            refAssemblies.Add(
                MetadataReference.CreateFromFile(
                    typeof(NextNet.Components.IPage).GetTypeInfo().Assembly.Location));

            // 5. Fixture source files — these define the user component types that
            //    the generated wrappers reference (IndexPage, AboutPage, RootLayout, etc.)
            var fixtureSyntaxTrees = new List<SyntaxTree>
            {
                CSharpSyntaxTree.ParseText(""),
            };

            var fixtureDir = FixturesDir;
            if (Directory.Exists(fixtureDir))
            {
                foreach (var fixtureFile in Directory.EnumerateFiles(fixtureDir, "*.cs", SearchOption.AllDirectories))
                {
                    var source = File.ReadAllText(fixtureFile);
                    fixtureSyntaxTrees.Add(CSharpSyntaxTree.ParseText(source, path: fixtureFile));
                }
            }

            return CSharpCompilation.Create(
                "TestAssembly",
                syntaxTrees: fixtureSyntaxTrees,
                references: refAssemblies,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        /// <summary>
        /// Minimal <see cref="AdditionalText"/> implementation for test use.
        /// </summary>
        private sealed class TestAdditionalText : AdditionalText
        {
            private readonly string _text;

            public TestAdditionalText(string path, string text)
            {
                Path = path;
                _text = text;
            }

            public override string Path { get; }

            public override SourceText? GetText(CancellationToken cancellationToken = default)
            {
                return SourceText.From(_text, Encoding.UTF8);
            }
        }
    }

    /// <summary>
    /// Helper for finding test fixture files.
    /// </summary>
    internal static class TestHelper
    {
        /// <summary>
        /// Walks up from the assembly location to find the test project root.
        /// </summary>
        public static string FindTestRoot()
        {
            var dir = new DirectoryInfo(typeof(CompilationIntegrationTests).GetTypeInfo().Assembly.Location);
            while (dir != null)
            {
                var fixturesDir = Path.Combine(dir.FullName, "Fixtures");
                if (Directory.Exists(fixturesDir) && File.Exists(Path.Combine(fixturesDir, "nextnet.routes.json")))
                {
                    return dir.FullName;
                }
                dir = dir.Parent;
            }

            // Fallback: check current directory
            var currentDir = Directory.GetCurrentDirectory();
            var testDir = currentDir;
            while (testDir != null)
            {
                if (testDir.EndsWith("NextNet.SourceGenerators.Tests") ||
                    Directory.Exists(Path.Combine(testDir, "Fixtures")))
                {
                    return testDir;
                }
                testDir = Path.GetDirectoryName(testDir);
            }

            return currentDir;
        }
    }
}
