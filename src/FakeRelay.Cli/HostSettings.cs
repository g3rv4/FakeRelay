using Spectre.Console.Cli;

namespace FakeRelay.Cli;

public class HostSettings : CommandSettings
{
    [CommandArgument(0, "<HOST>")]
    public string Host { get; set; }
}