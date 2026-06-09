using System.Collections.Generic;
using NextNet.SourceGenerators.Models;
using NextNet.SourceGenerators.Utils;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for helper methods in <see cref="IncrementalHelpers"/>.
    /// </summary>
    public class IncrementalHelpersTests
    {
        // ── FilePathToTypeName ──────────────────────────────────────────────

        [Theory]
        [InlineData("app/page.cs", "app", "IndexPage")]
        [InlineData("app/about/page.cs", "app", "AboutPage")]
        [InlineData("app/blog/[slug]/page.cs", "app", "BlogSlugPage")]
        [InlineData("app/blog/[...slug]/page.cs", "app", "BlogSlugPage")]
        [InlineData("app/blog/[[...slug]]/page.cs", "app", "BlogSlugPage")]
        [InlineData("app/layout.cs", "app", "RootLayout")]
        [InlineData("app/api/users/route.cs", "app", "ApiUsersRoute")]
        [InlineData("app/error.cs", "app", "ErrorPage")]
        [InlineData("app/blog/layout.cs", "app", "BlogLayout")]
        [InlineData("app/blog/[slug]/[comment]/page.cs", "app", "BlogSlugCommentPage")]
        public void FilePathToTypeName_ShouldReturnExpectedName_WhenPathIsValid(string filePath, string appDir, string expected)
        {
            var result = IncrementalHelpers.FilePathToTypeName(filePath, appDir);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(null, "app")]
        [InlineData("", "app")]
        public void FilePathToTypeName_ShouldReturnUnknown_WhenPathIsNullOrEmpty(string? filePath, string appDir)
        {
            var result = IncrementalHelpers.FilePathToTypeName(filePath ?? string.Empty, appDir);
            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void FilePathToTypeName_ShouldHandleCaseInsensitiveAppDir_WhenUsed()
        {
            var result = IncrementalHelpers.FilePathToTypeName("APP/Page.cs", "app");
            Assert.Equal("IndexPage", result);
        }

        [Fact]
        public void FilePathToTypeName_ShouldHandleBackslashSeparators_WhenUsed()
        {
            var result = IncrementalHelpers.FilePathToTypeName("app\\about\\page.cs", "app");
            Assert.Equal("AboutPage", result);
        }

        // ── ExtractRouteParameters ─────────────────────────────────────────

        [Theory]
        [InlineData("/blog/{slug}", new[] { "slug" })]
        [InlineData("/blog/{slug}/{comment}", new[] { "slug", "comment" })]
        [InlineData("/", new string[0])]
        [InlineData("/about", new string[0])]
        [InlineData("/{*path}", new[] { "path" })]
        [InlineData(null, new string[0])]
        [InlineData("", new string[0])]
        public void ExtractRouteParameters_ShouldReturnExpectedParams_WhenPatternIsValid(string? pattern, string[] expected)
        {
            var result = IncrementalHelpers.ExtractRouteParameters(pattern ?? string.Empty);
            Assert.Equal(expected, result);
        }

        // ── HasCatchAll ────────────────────────────────────────────────────

        [Theory]
        [InlineData("/{*path}", true)]
        [InlineData("/blog/{*slug}", true)]
        [InlineData("/", false)]
        [InlineData("/about", false)]
        [InlineData("/blog/{slug}", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void HasCatchAll_ShouldReturnExpected_WhenPatternIsValid(string? pattern, bool expected)
        {
            var result = IncrementalHelpers.HasCatchAll(pattern ?? string.Empty);
            Assert.Equal(expected, result);
        }

        // ── HasDynamicParams ───────────────────────────────────────────────

        [Theory]
        [InlineData("/blog/{slug}", true)]
        [InlineData("/{*path}", true)]
        [InlineData("/", false)]
        [InlineData("/about", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void HasDynamicParams_ShouldReturnExpected_WhenPatternIsValid(string? pattern, bool expected)
        {
            var result = IncrementalHelpers.HasDynamicParams(pattern ?? string.Empty);
            Assert.Equal(expected, result);
        }

        // ── GetWrapperName ─────────────────────────────────────────────────

        [Theory]
        [InlineData("app/page.cs", "app", "NextNet_IndexPage")]
        [InlineData("app/about/page.cs", "app", "NextNet_AboutPage")]
        [InlineData("app/api/users/route.cs", "app", "NextNet_ApiUsersRoute")]
        [InlineData("app/error.cs", "app", "NextNet_ErrorPage")]
        [InlineData("", "app", "NextNet_Route")]
        public void GetWrapperName_ShouldReturnExpectedName_WhenEntryIsValid(string filePath, string appDir, string expected)
        {
            var entry = new RouteEntryModel
            {
                RoutePattern = "/",
                FilePath = filePath,
                Type = "Page",
                SegmentKind = "Static"
            };

            var result = IncrementalHelpers.GetWrapperName(entry, appDir);
            Assert.Equal(expected, result);
        }

        // ── SanitizeSegment (tested indirectly via FilePathToTypeName) ─────

        [Fact]
        public void FilePathToTypeName_ShouldSanitizeSpecialChars_WhenSegmentContainsThem()
        {
            // Segments with special chars should be sanitized
            var result = IncrementalHelpers.FilePathToTypeName("app/my-page/page.cs", "app");
            Assert.Equal("MyPagePage", result);
        }
    }
}
