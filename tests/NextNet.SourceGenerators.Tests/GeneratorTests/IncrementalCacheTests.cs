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
        public void RouteManifestModelComparer_SameInstance_ReturnsTrue()
        {
            var manifest = new RouteManifestModel();
            var comparer = RouteManifestModelComparer.Instance;

            Assert.True(comparer.Equals(manifest, manifest));
        }

        [Fact]
        public void RouteManifestModelComparer_BothNull_ReturnsTrue()
        {
            var comparer = RouteManifestModelComparer.Instance;

            Assert.True(comparer.Equals(null, null));
        }

        [Fact]
        public void RouteManifestModelComparer_OneNull_ReturnsFalse()
        {
            var comparer = RouteManifestModelComparer.Instance;
            var manifest = new RouteManifestModel();

            Assert.False(comparer.Equals(manifest, null));
            Assert.False(comparer.Equals(null, manifest));
        }

        [Fact]
        public void RouteManifestModelComparer_IdenticalContent_ReturnsTrue()
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
        public void RouteManifestModelComparer_DifferentRoutePattern_ReturnsFalse()
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
        public void RouteManifestModelComparer_DifferentCounts_ReturnsFalse()
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
        public void RouteManifestModelComparer_ErrorPagePresent_WorksCorrectly()
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
        public void RouteManifestModelComparer_ErrorPageVsNull_ReturnsFalse()
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
        public void RouteManifestModelComparer_ConsistentHashCode()
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
        public void RouteManifestModelComparer_NullInput_GetHashCodeReturnsZero()
        {
            var comparer = RouteManifestModelComparer.Instance;
            var hashCode = comparer.GetHashCode(null!);

            Assert.Equal(0, hashCode);
        }

        [Fact]
        public void ComponentModel_ValueEquality_SameContent_AreEqual()
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
        public void ComponentModel_ValueEquality_DifferentTypeName_NotEqual()
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
        public void ComponentModel_ValueEquality_NullComparison_ReturnsFalse()
        {
            var a = new ComponentModel();
            Assert.False(a.Equals(null));
        }
    }
}
