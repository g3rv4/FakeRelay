using System.ComponentModel;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Settings;

public class ConfigSettings : CommandSettings
{
    [Description("The hostname of the relay.")]
    [CommandArgument(0, "<HOST>")]
    public string Host { get; set; }
}
