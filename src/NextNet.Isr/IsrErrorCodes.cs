namespace NextNet.Isr;

/// <summary>
/// Defines standardized error codes for the NextNet ISR (Incremental Static Regeneration) subsystem.
/// All error codes follow the DS-3xx format and are used in validation and runtime error messages.
/// </summary>
public static class IsrErrorCodes
{
    /// <summary>
    /// Revalidate value must be non-negative or null.
    /// </summary>
    public const string RevalidateMustBeNonNegative = "DS-300";

    /// <summary>
    /// MaxConcurrentRegenerations must be at least 1.
    /// </summary>
    public const string MaxConcurrentRegenerationsMustBeAtLeastOne = "DS-301";

    /// <summary>
    /// DefaultRevalidateSeconds must be non-negative.
    /// </summary>
    public const string DefaultRevalidateSecondsMustBeNonNegative = "DS-302";

    /// <summary>
    /// Global MaxConcurrentRegenerations must be at least 1.
    /// </summary>
    public const string GlobalMaxConcurrentRegenerationsMustBeAtLeastOne = "DS-303";

    /// <summary>
    /// MaxPendingRevalidations must be at least 1.
    /// </summary>
    public const string MaxPendingRevalidationsMustBeAtLeastOne = "DS-304";

    /// <summary>
    /// DeduplicationWindow must be non-negative.
    /// </summary>
    public const string DeduplicationWindowMustBeNonNegative = "DS-305";

    /// <summary>
    /// Revalidation interval must be non-negative.
    /// </summary>
    public const string RevalidationIntervalMustBeNonNegative = "DS-306";

    /// <summary>
    /// Base path does not exist.
    /// </summary>
    public const string BasePathDoesNotExist = "DS-307";

    /// <summary>
    /// HTTP method not allowed for this endpoint.
    /// </summary>
    public const string MethodNotAllowed = "DS-308";

    /// <summary>
    /// Request body could not be parsed.
    /// </summary>
    public const string InvalidRequestBody = "DS-309";

    /// <summary>
    /// Request body is required.
    /// </summary>
    public const string RequestBodyRequired = "DS-310";

    /// <summary>
    /// The provided secret is invalid or missing.
    /// </summary>
    public const string InvalidOrMissingSecret = "DS-311";

    /// <summary>
    /// Either a path or tags must be specified for revalidation.
    /// </summary>
    public const string PathOrTagsRequired = "DS-312";

    /// <summary>
    /// Revalidation operation failed.
    /// </summary>
    public const string RevalidationFailed = "DS-313";

    /// <summary>
    /// Revalidation was cancelled.
    /// </summary>
    public const string RevalidationCancelled = "DS-314";

    /// <summary>
    /// Revalidation failed for a specific route.
    /// </summary>
    public const string RevalidationFailedForRoute = "DS-315";

    /// <summary>
    /// No tags were provided for tag-based invalidation.
    /// </summary>
    public const string NoTagsProvidedForInvalidation = "DS-316";

    /// <summary>
    /// Tag-based invalidation failed.
    /// </summary>
    public const string TagBasedInvalidationFailed = "DS-317";

    /// <summary>
    /// Webhook signature verification failed.
    /// </summary>
    public const string InvalidWebhookSignature = "DS-318";
}
