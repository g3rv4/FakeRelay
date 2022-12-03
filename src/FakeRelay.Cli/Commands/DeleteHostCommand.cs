using FakeRelay.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public class DeleteHostCommand: AsyncCommand<HostSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, HostSettings settings)
    {
        await ApiKeysHelper.DeleteTokenForHostAsync(settings.Host);
        AnsiConsole.Markup($"[green]Key deleted for {settings.Host}[/]\n");
        return 0;
    }
}
