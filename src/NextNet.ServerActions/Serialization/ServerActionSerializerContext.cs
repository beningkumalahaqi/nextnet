using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NextNet.ServerActions.Serialization;

/// <summary>
/// Source-generated JSON serialization context for server action types.
/// Improves AOT compatibility and serialization performance.
/// When the source generator emits the full set of action parameter types,
/// those types should be added to this context's <see cref="JsonSerializableAttribute"/> list.
/// </summary>
[JsonSerializable(typeof(Results.ActionResult))]
[JsonSerializable(typeof(Results.ActionResult<object>))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[ExcludeFromCodeCoverage]
internal partial class ServerActionSerializerContext : JsonSerializerContext
{
}
