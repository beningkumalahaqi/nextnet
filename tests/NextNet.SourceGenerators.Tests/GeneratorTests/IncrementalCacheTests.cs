using System.Collections.Generic;
using NextNet.SourceGenerators.Models;
using NextNet.SourceGenerators.Utils;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for incremental caching value equality of pipeline models.
    /// Verifies that the comparer correctly identifies when inputs have/haven't changed.
    /// </summary>
    public class IncrementalCacheTests
    {
        [Fact]
        public void RouteManifestModelComparer_ShouldReturnTrue_WhenSameInstance()
        {
            var manifest = new RouteManifestModel();
            var comparer = RouteManifestModelComparer.Instance;

            Assert.True(comparer.Equals(manifest, manifest));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnTrue_WhenBothNull()
        {
            var comparer = RouteManifestModelComparer.Instance;

            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnFalse_WhenOneIsNull()
        {
            var comparer = RouteManifestModelComparer.Instance;
            var manifest = new RouteManifestModel();

            Assert.False(comparer.Equals(manifest, null));
            Assert.False(comparer.Equals(null, manifest));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnTrue_WhenContentIdentical()
        {
            var a = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var b = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var comparer = RouteManifestModelComparer.Instance;
            Assert.True(comparer.Equals(a, b));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnFalse_WhenRoutePatternDiffers()
        {
            var a = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var b = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/about", FilePath = "app/about/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var comparer = RouteManifestModelComparer.Instance;
            Assert.False(comparer.Equals(a, b));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnFalse_WhenEntryCountsDiffer()
        {
            var a = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var b = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" },
                    new RouteEntryModel { RoutePattern = "/about", FilePath = "app/about/page.cs", Type = "Page", SegmentKind = "Static" }
                }
            };

            var comparer = RouteManifestModelComparer.Instance;
            Assert.False(comparer.Equals(a, b));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnTrue_WhenErrorPageMatches()
        {
            var a = new RouteManifestModel
            {
                ErrorPage = new RouteEntryModel { RoutePattern = "/", FilePath = "app/error.cs", Type = "Error", SegmentKind = "Static" }
            };

            var b = new RouteManifestModel
            {
                ErrorPage = new RouteEntryModel { RoutePattern = "/", FilePath = "app/error.cs", Type = "Error", SegmentKind = "Static" }
            };

            var comparer = RouteManifestModelComparer.Instance;
            Assert.True(comparer.Equals(a, b));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnFalse_WhenErrorPageVsNull()
        {
            var a = new RouteManifestModel
            {
                ErrorPage = new RouteEntryModel { RoutePattern = "/", FilePath = "app/error.cs", Type = "Error", SegmentKind = "Static" }
            };

            var b = new RouteManifestModel();

            var comparer = RouteManifestModelComparer.Instance;
            Assert.False(comparer.Equals(a, b));
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldProduceConsistentHashCode_WhenContentIdentical()
        {
            var a = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                },
                ErrorPage = new RouteEntryModel { RoutePattern = "/", FilePath = "app/error.cs", Type = "Error", SegmentKind = "Static" }
            };

            var b = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>
                {
                    new RouteEntryModel { RoutePattern = "/", FilePath = "app/page.cs", Type = "Page", SegmentKind = "Static" }
                },
                ErrorPage = new RouteEntryModel { RoutePattern = "/", FilePath = "app/error.cs", Type = "Error", SegmentKind = "Static" }
            };

            var comparer = RouteManifestModelComparer.Instance;
            var hashA = comparer.GetHashCode(a);
            var hashB = comparer.GetHashCode(b);

            Assert.Equal(hashA, hashB);
        }

        [Fact]
        public void RouteManifestModelComparer_ShouldReturnZero_WhenGetHashCodeCalledOnNull()
        {
            var comparer = RouteManifestModelComparer.Instance;
            var hashCode = comparer.GetHashCode(null!);

            Assert.Equal(0, hashCode);
        }

        [Fact]
        public void ComponentModel_ShouldBeEqual_WhenContentIsSame()
        {
            var a = new ComponentModel
            {
                TypeName = "global::App.IndexPage",
                FilePath = "/app/page.cs",
                RoutePattern = "/",
                ComponentType = "Page"
            };

            var b = new ComponentModel
            {
                TypeName = "global::App.IndexPage",
                FilePath = "/app/page.cs",
                RoutePattern = "/",
                ComponentType = "Page"
            };

            Assert.Equal(a, b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void ComponentModel_ShouldNotBeEqual_WhenTypeNameDiffers()
        {
            var a = new ComponentModel
            {
                TypeName = "global::App.IndexPage",
                FilePath = "/app/page.cs",
                RoutePattern = "/",
                ComponentType = "Page"
            };

            var b = new ComponentModel
            {
                TypeName = "global::App.AboutPage",
                FilePath = "/app/about/page.cs",
                RoutePattern = "/about",
                ComponentType = "Page"
            };

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void ComponentModel_ShouldReturnFalse_WhenComparedToNull()
        {
            var a = new ComponentModel();
            Assert.False(a.Equals(null));
        }
    }
}
