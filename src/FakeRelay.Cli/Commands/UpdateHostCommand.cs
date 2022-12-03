using FakeRelay.Cli.Settings;
using FakeRelay.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public class UpdateHostCommand : ConfigEnabledAsyncCommand<HostSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, HostSettings settings)
    {
        var token = await ApiKeysHelper.UpdateTokenForHostAsync(settings.Host);
        AnsiConsole.Markup($"[green]Key generated for {settings.Host}[/]\n");
        AnsiConsole.Markup($"[red]{token}[/]\n");
        return 0;
    }
}
