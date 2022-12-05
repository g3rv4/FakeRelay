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
        
        // Create a table
        var table = new Table();
        
        table.AddColumn("Instance");
        table.AddColumn("Key");

        foreach (var group in hostToKeys)
        {
            var host = group.Key;
            foreach (var key in group)
            {
                table.AddRow($"[green]{host}[/]", $"[red]{key}[/]");
            }
        }
        
        AnsiConsole.Write(table);
        return 0;
    }
}
