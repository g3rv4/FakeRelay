using FakeRelay.Core.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public class ListInstancesCommand : ConfigEnabledAsyncCommand<EmptyCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EmptyCommandSettings settings)
    {
        var keyToHost = await ApiKeysHelper.GetTokenToHostAsync();
        var hostToKeys = keyToHost.ToLookup(d => d.Value, d => d.Key);
        var hostToNotes = await ApiKeysHelper.GetHostToNotesAsync();
        
        // Create a table
        var table = new Table();
        
        table.AddColumn("Instance");
        table.AddColumn("Notes");
        table.AddColumn("Key");

        foreach (var group in hostToKeys.OrderBy(g => g.Key))
        {
            var host = group.Key;
            foreach (var key in group)
            {
                var notes = hostToNotes.GetValueOrDefault(host) ?? "";
                table.AddRow($"[green]{host}[/]", notes , $"[red]{key}[/]");
            }
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
