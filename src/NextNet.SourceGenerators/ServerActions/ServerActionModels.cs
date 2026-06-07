using System.Collections.Generic;

namespace NextNet.SourceGenerators.ServerActions
{
    /// <summary>
    /// Represents a discovered server action method extracted from source code.
    /// </summary>
    internal sealed record ServerActionMethodModel
    {
        /// <summary>
        /// The action name (from the attribute's Name property or the method name).
        /// </summary>
        public string ActionName { get; set; } = string.Empty;

        /// <summary>
        /// The route for this action (e.g. "/_actions/CreateUser").
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// The fully qualified type name containing the method.
        /// </summary>
        public string DeclaringType { get; set; } = string.Empty;

        /// <summary>
        /// The method name.
        /// </summary>
        public string MethodName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the method is static.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Whether the action allows anonymous access.
        /// </summary>
        public bool AllowAnonymous { get; set; }

        /// <summary>
        /// The return type as a display string.
        /// </summary>
        public string ReturnTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Whether the return type is <c>void</c>.
        /// </summary>
        public bool ReturnsVoid { get; set; }

        /// <summary>
        /// Whether the return type is <c>Task</c> or <c>Task&lt;T&gt;</c>.
        /// </summary>
        public bool ReturnsTask { get; set; }

        /// <summary>
        /// If the return type is <c>Task&lt;T&gt;</c>, the inner type name.
        /// </summary>
        public string TaskResultTypeName { get; set; } = string.Empty;

        /// <summary>
        /// The method's parameters.
        /// </summary>
        public List<ServerActionParameterModel> Parameters { get; set; } = new();

        /// <summary>
        /// The declaring type's namespace.
        /// </summary>
        public string DeclaringNamespace { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a parameter of a server action method.
    /// </summary>
    internal sealed record ServerActionParameterModel
    {
        /// <summary>
        /// The parameter name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The fully qualified type name.
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Whether this parameter is typically resolved from DI.
        /// </summary>
        public bool IsService { get; set; }

        /// <summary>
        /// Whether this parameter is a <see cref="System.Threading.CancellationToken"/>.
        /// </summary>
        public bool IsCancellationToken { get; set; }
    }
}
