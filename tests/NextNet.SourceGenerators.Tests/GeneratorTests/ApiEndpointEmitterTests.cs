using System.Collections.Generic;
using NextNet.SourceGenerators.Emitters;
using NextNet.SourceGenerators.Models;
using Xunit;

namespace NextNet.SourceGenerators.Tests.GeneratorTests
{
    /// <summary>
    /// Tests for the <see cref="ApiEndpointEmitter"/> that generates MapGet/MapPost/MapPut/MapPatch/MapDelete
    /// registrations for API route handlers.
    /// </summary>
    public class ApiEndpointEmitterTests
    {
        private const string TestNs = "NextNet.Generated";

        private static RouteManifestModel CreateSampleManifest()
        {
            return new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>(),
                ApiRoutes = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/api/users",
                        FilePath = "app/api/users/route.cs",
                        Type = "Api",
                        SegmentKind = "Static",
                        HttpMethods = new List<string> { "GET", "POST" }
                    },
                    new RouteEntryModel
                    {
                        RoutePattern = "/api/products/{id}",
                        FilePath = "app/api/products/[id]/route.cs",
                        Type = "Api",
                        SegmentKind = "Dynamic",
                        HttpMethods = new List<string> { "GET", "PUT", "DELETE" }
                    }
                }
            };
        }

        [Fact]
        public void Emit_ShouldContainRegisterApiEndpointsMethod_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("RegisterApiEndpoints", result);
            Assert.Contains("WebApplication", result);
        }

        [Fact]
        public void Emit_ShouldContainMapGet_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapGet(\"/api/users\"", result);
        }

        [Fact]
        public void Emit_ShouldContainMapPost_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapPost(\"/api/users\"", result);
        }

        [Fact]
        public void Emit_ShouldContainMapPut_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapPut(\"/api/products/{id}\"", result);
        }

        [Fact]
        public void Emit_ShouldContainMapDelete_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapDelete(\"/api/products/{id}\"", result);
        }

        [Fact]
        public void Emit_ShouldNotContainUnregisteredMethods_WhenApiRoutesHaveLimitedMethods()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            // /api/users only has GET and POST
            Assert.DoesNotContain("MapDelete(\"/api/users\"", result);
        }

        [Fact]
        public void Emit_ShouldUseDIServiceResolution_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("GetRequiredService", result);
        }

        [Fact]
        public void Emit_ShouldSetHttpContext_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("route.HttpContext = context", result);
        }

        [Fact]
        public void Emit_ShouldCallPascalCaseMethods_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("return await route.Get()", result);
            Assert.Contains("return await route.Post()", result);
            Assert.Contains("return await route.Put(", result);
            Assert.Contains("return await route.Delete(", result);
        }

        [Fact]
        public void Emit_ShouldPassRouteParams_WhenDynamicRoute()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("string id", result);
            Assert.Contains("return await route.Get(id)", result);
            Assert.Contains("return await route.Put(id)", result);
            Assert.Contains("return await route.Delete(id)", result);
        }

        [Fact]
        public void Emit_ShouldUseWrapperTypes_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("NextNet_ApiUsersRoute", result);
            Assert.Contains("NextNet_ApiProductsIdRoute", result);
        }

        [Fact]
        public void Emit_ShouldUseGlobalPrefix_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("global::NextNet.Generated.NextNet_ApiUsersRoute", result);
        }

        [Fact]
        public void Emit_ShouldReturnEmptyRegistration_WhenManifestEmpty()
        {
            var manifest = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                ApiRoutes = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>()
            };

            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("RegisterApiEndpoints", result);
            Assert.Contains("No API routes registered", result);
            Assert.DoesNotContain("MapGet", result);
        }

        [Fact]
        public void Emit_ShouldGenerateAllFiveMethods_WhenAllHttpMethods()
        {
            var manifest = new RouteManifestModel
            {
                Pages = new List<RouteEntryModel>(),
                Layouts = new List<RouteEntryModel>(),
                Routes = new List<RouteEntryModel>(),
                Conflicts = new List<RouteConflictModel>(),
                ApiRoutes = new List<RouteEntryModel>
                {
                    new RouteEntryModel
                    {
                        RoutePattern = "/api/all",
                        FilePath = "app/api/all/route.cs",
                        Type = "Api",
                        SegmentKind = "Static"
                        // No HttpMethods specified — defaults to all 5
                    }
                }
            };

            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("MapGet(\"/api/all\"", result);
            Assert.Contains("MapPost(\"/api/all\"", result);
            Assert.Contains("MapPut(\"/api/all\"", result);
            Assert.Contains("MapPatch(\"/api/all\"", result);
            Assert.Contains("MapDelete(\"/api/all\"", result);
        }

        [Fact]
        public void Emit_ShouldContainAutoGeneratedHeader_WhenCalled()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            Assert.Contains("<auto-generated />", result);
        }

        [Fact]
        public void Emit_ShouldGenerateWithoutNamespace_WhenNamespaceNullEmpty()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", string.Empty);

            Assert.DoesNotContain("namespace ", result);
        }

        [Fact]
        public void Emit_ShouldIncludeContextSignature_WhenApiRoutes()
        {
            var manifest = CreateSampleManifest();
            var result = ApiEndpointEmitter.Emit(manifest, "app", TestNs);

            // All endpoint lambdas should include HttpContext in the signature
            Assert.Contains("HttpContext context", result);
        }
    }
}
