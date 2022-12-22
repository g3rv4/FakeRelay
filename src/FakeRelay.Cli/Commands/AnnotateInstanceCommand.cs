using FakeRelay.Cli.Settings;
using FakeRelay.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public class AnnotateInstanceCommand : ConfigEnabledAsyncCommand<AnnotateInstanceSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, AnnotateInstanceSettings settings)
    {
        await ApiKeysHelper.UpdateNotesForHostAsync(settings.Host, settings.Notes);
        AnsiConsole.Markup("[green]Done![/]\n");
        return 0;
    }
}
