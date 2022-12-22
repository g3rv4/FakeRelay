using Spectre.Console.Cli;

namespace FakeRelay.Cli.Settings;

public class AddInstanceSettings : InstanceSettings
{
    [CommandOption("-n|--notes")]
    public string? Notes { get; set; }
}
