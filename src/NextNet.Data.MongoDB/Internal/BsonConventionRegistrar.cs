namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Registers the default BSON serialization convention pack for the MongoDB provider.
/// Ensures conventions are registered exactly once across the application lifetime.
/// </summary>
/// <remarks>
/// <para>
/// The default convention pack applies:
/// <list type="bullet">
///   <item><description><b>CamelCaseElementNameConvention</b> — maps CLR property names to camelCase BSON element names</description></item>
///   <item><description><b>IgnoreExtraElementsConvention</b> — ignores extra BSON elements not present in the CLR class</description></item>
///   <item><description><b>NamedIdMemberConvention</b> — recognizes <c>Id</c> or <c>{TypeName}Id</c> as the document <c>_id</c></description></item>
///   <item><description><b>StringObjectIdIdGeneratorConvention</b> — automatically generates <c>ObjectId</c> values for string <c>Id</c> properties</description></item>
/// </list>
/// </para>
/// <para>
/// Registration is thread-safe via a static guard flag. Once registered, subsequent
/// calls are no-ops.
/// </para>
/// </remarks>
internal static class BsonConventionRegistrar
{
    private static readonly object Lock = new();
    private static volatile bool _registered;

    /// <summary>
    /// Registers the default BSON convention pack if not already registered.
    /// </summary>
    /// <param name="options">Optional convention customization options.</param>
    public static void Register(MongoDbConventionOptions? options = null)
    {
        if (_registered)
        {
            return;
        }

        lock (Lock)
        {
            if (_registered)
            {
                return;
            }

            options ??= new MongoDbConventionOptions();
            var pack = new ConventionPack();

            if (options.UseCamelCaseElementNames)
            {
                pack.Add(new CamelCaseElementNameConvention());
            }

            if (options.IgnoreExtraElements)
            {
                pack.Add(new IgnoreExtraElementsConvention(true));
            }

            if (options.AutoMapIdToUnderscoreId)
            {
                pack.Add(new NamedIdMemberConvention());
                pack.Add(new StringObjectIdIdGeneratorConvention());
            }

            // Add any custom convention packs
            if (options.AdditionalConventionPacks is not null)
            {
                foreach (var additionalPack in options.AdditionalConventionPacks)
                {
                    pack.AddRange(additionalPack.Conventions);
                }
            }

            ConventionRegistry.Register(
                "NextNet MongoDB Provider",
                pack,
                _ => true); // Apply to all types

            _registered = true;
        }
    }

    /// <summary>
    /// Gets whether the BSON conventions have been registered.
    /// </summary>
    internal static bool IsRegistered => _registered;

    /// <summary>
    /// Resets the registration state. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        _registered = false;
    }
}
