using System.Reflection;
using System.Reflection.Emit;

namespace NextNet.Plugins.Tests.TestInfrastructure;

/// <summary>
/// Builds dynamic assemblies with <see cref="NextNetPluginAttribute"/> for testing
/// the plugin discovery and loading mechanism without needing physical DLLs.
/// </summary>
public static class TestPluginBuilder
{
    private static int _assemblyCounter;

    /// <summary>
    /// Builds a dynamic assembly with a <see cref="NextNetPluginAttribute"/>
    /// pointing to the specified plugin type.
    /// </summary>
    /// <param name="pluginName">The plugin name to embed in the attribute.</param>
    /// <param name="pluginVersion">The plugin version string.</param>
    /// <param name="pluginType">The type that implements <see cref="INextNetPlugin"/>.</param>
    /// <returns>The constructed assembly with the attribute applied.</returns>
    public static Assembly BuildPluginAssembly(
        string pluginName,
        string pluginVersion,
        Type pluginType)
    {
        var assemblyName = new AssemblyName($"TestPluginAssembly_{++_assemblyCounter}");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
            assemblyName,
            AssemblyBuilderAccess.Run);

        var constructorBuilder = typeof(NextNetPluginAttribute)
            .GetConstructors()
            .First();

        var attributeBuilder = new CustomAttributeBuilder(
            constructorBuilder,
            new object[] { pluginType, pluginName, pluginVersion });

        assemblyBuilder.SetCustomAttribute(attributeBuilder);

        return assemblyBuilder;
    }
}
