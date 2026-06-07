namespace NextNet.Cli.Interactive;

/// <summary>
/// Provides static methods for presenting interactive prompts to the user via the console.
/// Supports free-form text input, numbered choice selection, and yes/no confirmation prompts.
/// </summary>
/// <remarks>
/// <para>
/// All prompt methods read from <see cref="Console.In"/> and write to <see cref="Console.Out"/>.
/// Validation is performed via the <see cref="PromptDefinition.Validator"/> delegate when provided.
/// </para>
/// <para>
/// Choice prompts display numbered options with the default value marked with an asterisk.
/// Yes/no prompts display the default choice in uppercase (e.g., "Y/n" or "y/N").
/// </para>
/// </remarks>
public static class ProjectPrompts
{
    /// <summary>
    /// Prompts the user for a free-form text value with validation.
    /// Loops until the user provides a valid value or an empty input with a default.
    /// </summary>
    /// <param name="definition">The prompt definition describing the expected input.</param>
    /// <param name="initialValue">An optional initial value to use if the user enters nothing.</param>
    /// <returns>The validated input value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    public static string PromptProjectName(PromptDefinition definition, string? initialValue = null)
    {
        ArgumentNullException.ThrowIfNull(definition);

        while (true)
        {
            Console.Write($"{definition.Description}: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input) && initialValue is not null)
                input = initialValue;

            if (definition.Validator is not null)
            {
                var error = definition.Validator(input ?? "");
                if (error is not null)
                {
                    Console.WriteLine($"  \u2717 {error}");
                    continue;
                }
            }

            if (string.IsNullOrEmpty(input))
            {
                if (definition.Required)
                {
                    Console.WriteLine("  \u2717 This field is required.");
                    continue;
                }
                input = definition.DefaultValue;
            }

            return input!;
        }
    }

    /// <summary>
    /// Prompts the user to select from a list of choices by number or by entering a value directly.
    /// </summary>
    /// <param name="definition">The prompt definition containing the allowed choices.</param>
    /// <returns>The selected value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="definition"/> has no <see cref="PromptDefinition.AllowedValues"/>.</exception>
    public static string PromptChoice(PromptDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.AllowedValues is null || definition.AllowedValues.Count == 0)
            throw new InvalidOperationException("Choice prompt requires AllowedValues");

        Console.WriteLine($"{definition.Description}:");
        for (int i = 0; i < definition.AllowedValues.Count; i++)
        {
            var marker = definition.AllowedValues[i] == definition.DefaultValue ? "*" : " ";
            Console.WriteLine($"  {i + 1}. {definition.AllowedValues[i]} {marker}");
        }

        var defaultIdx = definition.DefaultValue is not null
            ? Array.IndexOf(definition.AllowedValues.ToArray(), definition.DefaultValue) + 1
            : 1;
        Console.Write($"Select [{defaultIdx}]: ");

        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input))
            return definition.DefaultValue ?? definition.AllowedValues[0];

        if (int.TryParse(input, out var idx) && idx >= 1 && idx <= definition.AllowedValues.Count)
        {
            return definition.AllowedValues[idx - 1];
        }

        // Allow free-form input (e.g., typing "postgres" directly)
        return input;
    }

    /// <summary>
    /// Prompts the user for a yes/no confirmation.
    /// </summary>
    /// <param name="definition">The prompt definition containing the question and default.</param>
    /// <returns><c>true</c> if the user answered yes; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="definition"/> is null.</exception>
    public static bool PromptYesNo(PromptDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var defaultStr = definition.DefaultValue?.ToLowerInvariant() ?? "n";
        var defaultDisplay = defaultStr is "true" or "yes" or "y" ? "Y/n" : "y/N";
        Console.Write($"{definition.Description} [{defaultDisplay}]: ");
        var input = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (string.IsNullOrEmpty(input))
            input = defaultStr;

        return input is "y" or "yes" or "true" or "1";
    }
}
