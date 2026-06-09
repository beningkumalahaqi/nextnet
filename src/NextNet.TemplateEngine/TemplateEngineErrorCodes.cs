namespace NextNet.TemplateEngine;

/// <summary>
/// Defines error code constants used throughout the NextNet.TemplateEngine package.
/// </summary>
/// <remarks>
/// <para>
/// Error codes follow the DS-7xx naming convention, where "DS" stands for
/// "Design System / Templates" and the numeric suffix identifies the specific error.
/// </para>
/// <list type="table">
///   <listheader>
///     <term>Code</term>
///     <description>Error</description>
///   </listheader>
///   <item><term>DS-700</term><description>Conditional expression parse error.</description></item>
///   <item><term>DS-701</term><description>Conditional expression evaluation error.</description></item>
///   <item><term>DS-702</term><description>Invalid template variable type.</description></item>
///   <item><term>DS-703</term><description>Variable replacement error.</description></item>
///   <item><term>DS-704</term><description>Template generation options error.</description></item>
///   <item><term>DS-705</term><description>Template engine runtime error.</description></item>
///   <item><term>DS-706</term><description>Feature resolution error.</description></item>
///   <item><term>DS-707</term><description>Binary detection error.</description></item>
///   <item><term>DS-708</term><description>Conditional file filter error.</description></item>
///   <item><term>DS-709</term><description>Service registration error.</description></item>
/// </list>
/// </remarks>
public static class TemplateEngineErrorCodes
{
    /// <summary>
    /// Error code for conditional expression parse errors (DS-700).
    /// </summary>
    public const string ParseError = "DS-700";

    /// <summary>
    /// Error code for conditional expression evaluation errors (DS-701).
    /// </summary>
    public const string EvaluationError = "DS-701";

    /// <summary>
    /// Error code for invalid template variable type (DS-702).
    /// </summary>
    public const string InvalidVariableType = "DS-702";

    /// <summary>
    /// Error code for variable replacement errors (DS-703).
    /// </summary>
    public const string VariableReplacementError = "DS-703";

    /// <summary>
    /// Error code for template generation options errors (DS-704).
    /// </summary>
    public const string GenerationOptionsError = "DS-704";

    /// <summary>
    /// Error code for template engine runtime errors (DS-705).
    /// </summary>
    public const string EngineRuntimeError = "DS-705";

    /// <summary>
    /// Error code for feature resolution errors (DS-706).
    /// </summary>
    public const string FeatureResolutionError = "DS-706";

    /// <summary>
    /// Error code for binary detection errors (DS-707).
    /// </summary>
    public const string BinaryDetectionError = "DS-707";

    /// <summary>
    /// Error code for conditional file filter errors (DS-708).
    /// </summary>
    public const string ConditionalFileFilterError = "DS-708";

    /// <summary>
    /// Error code for service registration errors (DS-709).
    /// </summary>
    public const string ServiceRegistrationError = "DS-709";
}
