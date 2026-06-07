using System.Collections.Generic;
using NextNet.SourceGenerators.Emitters;
using NextNet.SourceGenerators.Models;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for the <see cref="WrapperEmitter"/> that generates wrapper classes.
    /// </summary>
    public class WrapperEmitterTests
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
                Layouts = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/",
                        FilePath = "app/layout.cs",
                        Type = "Layout",
                        SegmentKind = "Static"
                    },
                    new RouteEntryModel
                    {
                        RoutePattern = "/blog",
                        FilePath = "app/blog/layout.cs",
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
        public void Emit_WithSampleManifest_ContainsPageWrappers()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_IndexPage", result);
            Assert.Contains("NextNet_AboutPage", result);
            Assert.Contains("NextNet_BlogSlugPage", result);
        }

        [Fact]
        public void Emit_WithSampleManifest_ContainsLayoutWrappers()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_RootLayout", result);
            Assert.Contains("NextNet_BlogLayout", result);
        }

        [Fact]
        public void Emit_WithSampleManifest_ContainsApiRouteWrapper()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_ApiUsersRoute", result);
        }

        [Fact]
        public void Emit_WithSampleManifest_ContainsErrorPageWrapper()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_ErrorPage", result);
        }

        [Fact]
        public void Emit_PageWrapper_ImplementsIPage()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_IndexPage : global::NextNet.Components.IPage", result);
            Assert.Contains("NextNet_AboutPage : global::NextNet.Components.IPage", result);
        }

        [Fact]
        public void Emit_LayoutWrapper_ImplementsILayout()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_RootLayout : global::NextNet.Components.ILayout", result);
        }

        [Fact]
        public void Emit_PageWrapper_HasRenderMethod()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("Task<global::NextNet.Components.IHtmlContent> Render()", result);
        }

        [Fact]
        public void Emit_ErrorPageWrapper_ImplementsIErrorPage()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_ErrorPage : global::NextNet.Components.IErrorPage", result);
        }

        [Fact]
        public void Emit_DynamicPageWrapper_AssignsRouteParams()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            // The blog slug page wrapper should assign the slug property
            Assert.Contains("Slug", result);
        }

        [Fact]
        public void Emit_EmptyManifest_ReturnsNoWrappers()
        {
            var manifest = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                ApiRoutes = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };

            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.DoesNotContain("NextNet_", result);
        }

        [Fact]
        public void Emit_HasAutoGeneratedHeader()
        {
            var manifest = CreateSampleManifest();
            var result = WrapperEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("<auto-generated />", result);
        }
    }
}
