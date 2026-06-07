using System;
using System.Collections.Generic;
using NextNet.SourceGenerators.Models;

namespace NextNet.SourceGenerators.Utils
{
    /// <summary>
    /// A structural equality comparer for <see cref="RouteManifestModel"/>.
    /// Used by the incremental pipeline's <c>WithComparer</c> to enable caching
    /// when the manifest content has not changed.
    /// </summary>
    internal sealed class RouteManifestModelComparer : IEqualityComparer<RouteManifestModel>
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static readonly RouteManifestModelComparer Instance = new RouteManifestModelComparer();

        private RouteManifestModelComparer()
        {
        }

        public bool Equals(RouteManifestModel? x, RouteManifestModel? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return ListsEqual(x.Routes, y.Routes)
                && ListsEqual(x.Pages, y.Pages)
                && ListsEqual(x.Layouts, y.Layouts)
                && ListsEqual(x.ApiRoutes, y.ApiRoutes)
                && RouteEntryEquals(x.ErrorPage, y.ErrorPage)
                && ListsEqual(x.Conflicts, y.Conflicts);
        }

        public int GetHashCode(RouteManifestModel obj)
        {
            if (obj == null)
                return 0;

            var hash = 17;
            hash = CombineHash(hash, GetEntryListHash(obj.Routes));
            hash = CombineHash(hash, GetEntryListHash(obj.Pages));
            hash = CombineHash(hash, GetEntryListHash(obj.Layouts));
            hash = CombineHash(hash, GetEntryListHash(obj.ApiRoutes));
            hash = CombineHash(hash, obj.ErrorPage?.RoutePattern != null
                ? StringComparer.Ordinal.GetHashCode(obj.ErrorPage.RoutePattern)
                : 0);
            hash = CombineHash(hash, GetConflictListHash(obj.Conflicts));
            return hash;
        }

        private static bool ListsEqual<T>(List<T>? a, List<T>? b) where T : class
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;
            if (a.Count != b.Count)
                return false;

            for (var i = 0; i < a.Count; i++)
            {
                if (!Equals(a[i], b[i]))
                    return false;
            }

            return true;
        }

        private static bool RouteEntryEquals(RouteEntryModel? a, RouteEntryModel? b)
        {
            if (a == null && b == null)
                return true;
            if (a == null || b == null)
                return false;

            return string.Equals(a.RoutePattern, b.RoutePattern, StringComparison.Ordinal)
                && string.Equals(a.FilePath, b.FilePath, StringComparison.OrdinalIgnoreCase)
                && string.Equals(a.Type, b.Type, StringComparison.Ordinal)
                && string.Equals(a.SegmentKind, b.SegmentKind, StringComparison.Ordinal);
        }

        private static int GetEntryListHash(List<RouteEntryModel>? list)
        {
            if (list == null)
                return 0;

            var hash = list.Count;
            foreach (var item in list)
            {
                if (item != null)
                {
                    hash = CombineHash(hash, StringComparer.Ordinal.GetHashCode(item.RoutePattern ?? string.Empty));
                    hash = CombineHash(hash, StringComparer.OrdinalIgnoreCase.GetHashCode(item.FilePath ?? string.Empty));
                    hash = CombineHash(hash, StringComparer.Ordinal.GetHashCode(item.Type ?? string.Empty));
                    hash = CombineHash(hash, StringComparer.Ordinal.GetHashCode(item.SegmentKind ?? string.Empty));
                }
                else
                {
                    hash = CombineHash(hash, 0);
                }
            }

            return hash;
        }

        private static int GetConflictListHash(List<RouteConflictModel>? list)
        {
            if (list == null)
                return 0;

            var hash = list.Count;
            foreach (var item in list)
            {
                if (item != null)
                {
                    hash = CombineHash(hash, StringComparer.Ordinal.GetHashCode(item.Message ?? string.Empty));
                    hash = CombineHash(hash, StringComparer.Ordinal.GetHashCode(item.RoutePattern ?? string.Empty));
                }
                else
                {
                    hash = CombineHash(hash, 0);
                }
            }

            return hash;
        }

        private static int CombineHash(int h1, int h2)
        {
            // FNV-1a-like mixing
            unchecked
            {
                var hash = 17;
                hash = hash * 31 + h1;
                hash = hash * 31 + h2;
                return hash;
            }
        }
    }
}
