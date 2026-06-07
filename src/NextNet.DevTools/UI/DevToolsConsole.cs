namespace NextNet.DevTools.UI;

/// <summary>
/// Provides formatted console output helpers for DevTools.
/// </summary>
public static class DevToolsConsole
{
    /// <summary>Write a line to stdout with a timestamp prefix.</summary>
    public static void WriteLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        System.Console.WriteLine($"[{timestamp}] {message}");
    }

    /// <summary>Write a success message.</summary>
    public static void WriteSuccess(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Green;
        System.Console.WriteLine($"✓ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write a warning message.</summary>
    public static void WriteWarning(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"⚠ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write an error message.</summary>
    public static void WriteError(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine($"✗ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write an info message.</summary>
    public static void WriteInfo(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine($"ℹ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write a heading with underline.</summary>
    public static void WriteHeading(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(message);
        System.Console.WriteLine(new string('=', message.Length));
        System.Console.ResetColor();
    }

    /// <summary>Write a muted/subtle line.</summary>
    public static void WriteMuted(string message)
    {
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }
}
