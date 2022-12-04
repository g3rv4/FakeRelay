using System.ComponentModel;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Settings;

public class HostSettings : EmptyBaseSettings
{
    [Description("The instance that connects to this FakeRelay.")]
    [CommandArgument(0, "<INSTANCE_HOST>")]
    public string Host { get; set; }
}
