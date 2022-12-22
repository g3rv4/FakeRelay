using System.ComponentModel;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Settings;

public class AnnotateInstanceSettings : InstanceSettings
{
    [Description("The notes for the instance.")]
    [CommandArgument(1, "<NOTES>")]
    public string Notes { get; set; }

    public AnnotateInstanceSettings()
    {
        Notes = "";
    }
}
