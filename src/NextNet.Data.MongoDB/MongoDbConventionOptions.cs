namespace NextNet.Data.MongoDB;

/// <summary>
/// Configures the BSON serialization convention pack used by the MongoDB provider.
/// Applied globally at startup via <c>UseMongoDB()</c>.
/// </summary>
/// <remarks>
/// <para>
/// The default convention pack applies:
/// <list type="bullet">
///   <item><description><b>CamelCaseElementNameConvention</b> — maps CLR property names to camelCase BSON element names</description></item>
///   <item><description><b>IgnoreExtraElementsConvention</b> — ignores extra BSON elements not present in the CLR class</description></item>
///   <item><description><b>NamedIdMemberConvention</b> — recognizes <c>Id</c> or <c>{TypeName}Id</c> as the document <c>_id</c></description></item>
///   <item><description><b>StringObjectIdIdGeneratorConvention</b> — automatically generates <c>ObjectId</c> values for string <c>Id</c> properties with <c>[BsonRepresentation(BsonType.ObjectId)]</c></description></item>
/// </list>
/// </para>
/// <para>
/// Custom conventions can be added via <see cref="AdditionalConventionPacks"/>.
/// Conventions are registered once during provider initialization.
/// </para>
/// </remarks>
public sealed record MongoDbConventionOptions
{
    /// <summary>
    /// Gets or sets whether to apply the camelCase element name convention.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool UseCamelCaseElementNames { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to apply the ignore extra elements convention.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool IgnoreExtraElements { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to automatically map <c>Id</c> or <c>{TypeName}Id</c>
    /// properties to <c>_id</c>. Defaults to <c>true</c>.
    /// </summary>
    public bool AutoMapIdToUnderscoreId { get; set; } = true;

    /// <summary>
    /// Gets or sets custom convention packs to apply after defaults.
    /// </summary>
    public IReadOnlyList<IConventionPack>? AdditionalConventionPacks { get; set; }
}
