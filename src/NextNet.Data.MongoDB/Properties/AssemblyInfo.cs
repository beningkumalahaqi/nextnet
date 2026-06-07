using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NextNet.Data.MongoDB.Tests")]
[assembly: InternalsVisibleTo("NextNet.Cli")]
[assembly: InternalsVisibleTo("NextNet.Cli.Tests")]

// Allow the test project to access internal types for unit testing.
// In a development environment without strong naming, this is sufficient.
