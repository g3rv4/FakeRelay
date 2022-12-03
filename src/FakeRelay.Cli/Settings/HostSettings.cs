using System.ComponentModel;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Settings;

public class HostSettings : CommandSettings
{
    [Description("The instance that connects to this fake relay.")]
    [CommandArgument(0, "<HOST>")]
    public string Host { get; set; }
}
