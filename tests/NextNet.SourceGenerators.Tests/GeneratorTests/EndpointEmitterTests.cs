using System.Collections.Generic;
using System.Linq;
using NextNet.SourceGenerators.Emitters;
using NextNet.SourceGenerators.Models;
using NextNet.SourceGenerators.Utils;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for the <see cref="EndpointEmitter"/> that generates MapGet/MapPost registration.
    /// </summary>
    public class EndpointEmitterTests
    {
        private const string TestNs = "NextNet.Generated";

        private static RouteManifestModel CreateSampleManifest()
        {
            return new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/",
                        FilePath = "app/page.cs",
                        Type = "Page",
                        SegmentKind = "Static"
                    },
                    new RouteEntryModel
                    {
                        RoutePattern = "/about",
                        FilePath = "app/about/page.cs",
                        Type = "Page",
                        SegmentKind = "Static"
                    },
                    new RouteEntryModel
                    {
                        RoutePattern = "/blog/{slug}",
                        FilePath = "app/blog/[slug]/page.cs",
                        Type = "Page",
                        SegmentKind = "Dynamic"
                    }
                },
                ApiRoutes = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/api/users",
                        FilePath = "app/api/users/route.cs",
                        Type = "Api",
                        SegmentKind = "Static"
                    }
                },
                ErrorPage = new RouteEntryModel
                {
                    RoutePattern = "/",
                    FilePath = "app/error.cs",
                    Type = "Error",
                    SegmentKind = "Static"
                },
                Layouts = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };
        }

        [Fact]
        public void Emit_ShouldContainMapGet_WhenStaticPages()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapGet(\"/\"", result);
            Assert.Contains("MapGet(\"/about\"", result);
        }

        [Fact]
        public void Emit_ShouldContainMapGet_WhenDynamicPages()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapGet(\"/blog/{slug}\"", result);
            Assert.Contains("string slug", result);
        }

        [Fact]
        public void Emit_ShouldContainApiEndpoints_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapGet(\"/api/users\"", result);
            Assert.Contains("MapPost(\"/api/users\"", result);
        }

        [Fact]
        public void Emit_ShouldContainErrorPage_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("Map(\"/{*path}\"", result);
            Assert.Contains("catch-all", result.ToLowerInvariant());
        }

        [Fact]
        public void Emit_ShouldContainRegisterEndpointsMethod_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("RegisterEndpoints", result);
            Assert.Contains("WebApplication", result);
        }

        [Fact]
        public void Emit_ShouldUseWrapperTypes_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_IndexPage", result);
            Assert.Contains("NextNet_AboutPage", result);
            Assert.Contains("NextNet_BlogSlugPage", result);
            Assert.Contains("NextNet_ApiUsersRoute", result);
        }

        [Fact]
        public void Emit_ShouldReturnResultsContent_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("Results.Content(", result);
            Assert.Contains("text/html", result);
        }

        [Fact]
        public void Emit_ShouldUseGlobalPrefix_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            // Should reference wrapper types with global:: prefix
            Assert.Contains("global::NextNet.Generated.NextNet_IndexPage", result);
        }

        [Fact]
        public void Emit_ShouldReturnEmptyEndpointClass_WhenManifestEmpty()
        {
            var manifest = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                ApiRoutes = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };

            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("RegisterEndpoints", result);
            Assert.DoesNotContain("MapGet", result);
        }

        [Fact]
        public void Emit_ShouldContainAutoGeneratedHeader_WhenCalled()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("<auto-generated />", result);
        }

        [Fact]
        public void Emit_ShouldGenerateWithoutNamespace_WhenNamespaceNullEmpty()
        {
            var manifest = CreateSampleManifest();
            var result = EndpointEmitter.Emit(manifest, "app", string.Empty);

            Assert.DoesNotContain("namespace ", result);
        }
    }
}
