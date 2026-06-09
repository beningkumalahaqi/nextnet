using System.CommandLine;
using TemplateSdkCreate = NextNet.TemplateSdk.CLI.TemplateCreateCommand;
using TemplateSdkValidate = NextNet.TemplateSdk.CLI.TemplateValidateCommand;
using TemplateSdkPackage = NextNet.TemplateSdk.CLI.TemplatePackageCommand;
using TemplateSdkPublish = NextNet.TemplateSdk.CLI.TemplatePublishCommand;
using TemplateSdkLogin = NextNet.TemplateSdk.CLI.TemplateLoginCommand;

namespace NextNet.Cli.Commands.Template;

/// <summary>
/// Implements the <c>nextnet template</c> command group — manages project templates.
/// </summary>
public sealed class TemplateCommand : Command
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateCommand"/> class.
    /// </summary>
    public TemplateCommand() : base("template", "Manage templates")
    {
        AddCommand(new TemplateListCommand());
        AddCommand(new TemplateInfoCommand());
        AddCommand(new TemplateInstallCommand());
        AddCommand(new TemplateUpdateCommand());
        AddCommand(new TemplateRemoveCommand());
        AddCommand(new TemplateSdkCreate());
        AddCommand(new TemplateSdkValidate());
        AddCommand(new TemplateSdkPackage());
        AddCommand(new TemplateSdkPublish());
        AddCommand(new TemplateSdkLogin());
    }
}
