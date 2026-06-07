namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a single file produced by the scaffolding system.
/// </summary>
/// <param name="FilePath">The absolute path to the generated file.</param>
/// <param name="RelativePath">The path relative to the project root.</param>
/// <param name="ArtifactType">The type of artifact (Model, Repository, or Action).</param>
/// <param name="EntityName">The entity name that was scaffolded.</param>
/// <param name="LinesOfCode">The number of lines in the generated file.</param>
/// <param name="WasSkipped">Whether the file was skipped (existing file and OverwriteExisting was false).</param>
/// <param name="ErrorMessage">If generation failed, a description of the error.</param>
public sealed record ScaffoldArtifact(
    string FilePath,
    string RelativePath,
    ScaffoldArtifactType ArtifactType,
    string EntityName,
    int LinesOfCode,
    bool WasSkipped = false,
    string? ErrorMessage = null
);

/// <summary>
/// Classifies the type of scaffolded artifact.
/// </summary>
public enum ScaffoldArtifactType
{
    /// <summary>A model/entity class file.</summary>
    Model,
    /// <summary>A repository class file.</summary>
    Repository,
    /// <summary>A server action file for CRUD operations.</summary>
    Action,
    /// <summary>An admin page file (List, Detail, Create, Edit, Delete, or Layout).</summary>
    AdminPage
}
