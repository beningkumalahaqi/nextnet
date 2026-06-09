using System.Collections.Generic;
using NextNet.SourceGenerators.Emitters;
using NextNet.SourceGenerators.Models;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for the <see cref="DiEmitter"/> that generates DI registration.
    /// </summary>
    public class DiEmitterTests
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
                    }
                },
                Layouts = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/",
                        FilePath = "app/layout.cs",
                        Type = "Layout",
                        SegmentKind = "Static"
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
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };
        }

        [Fact]
        public void Emit_ShouldContainRegisterServicesMethod_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("RegisterServices", result);
            Assert.Contains("IServiceCollection", result);
        }

        [Fact]
        public void Emit_ShouldRegisterPages_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("AddTransient<global::NextNet.Generated.NextNet_IndexPage>", result);
            Assert.Contains("AddTransient<global::NextNet.Generated.NextNet_AboutPage>", result);
        }

        [Fact]
        public void Emit_ShouldRegisterLayouts_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("AddTransient<global::NextNet.Generated.NextNet_RootLayout>", result);
        }

        [Fact]
        public void Emit_ShouldRegisterApiRoutes_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("AddScoped<global::NextNet.Generated.NextNet_ApiUsersRoute>", result);
        }

        [Fact]
        public void Emit_ShouldRegisterErrorPage_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("AddTransient<global::NextNet.Generated.NextNet_ErrorPage>", result);
        }

        [Fact]
        public void Emit_ShouldUseNextNetServiceRegistry_WhenSampleManifest()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("class NextNetServiceRegistry", result);
        }

        [Fact]
        public void Emit_ShouldRegisterNothing_WhenManifestEmpty()
        {
            var manifest = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                ApiRoutes = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };

            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("No routes registered", result);
            Assert.DoesNotContain("AddTransient", result);
        }

        [Fact]
        public void Emit_ShouldContainAutoGeneratedHeader_WhenCalled()
        {
            var manifest = CreateSampleManifest();
            var result = DiEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("<auto-generated />", result);
        }
    }
}
