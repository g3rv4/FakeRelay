using FakeRelay.Cli.Commands;
using FakeRelay.Cli.Settings;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch<EmptyBaseSettings>("host", host =>
    {
        host.AddCommand<AddHostCommand>("add")
            .WithDescription("Adds a host to the relay and generates a key.");
        host.AddCommand<UpdateHostCommand>("update")
            .WithDescription("Generates a new key for the host. The old one can't be used anymore.");
        host.AddCommand<DeleteHostCommand>("delete")
            .WithDescription("Deletes the existing keys for the host. They can't use FakeRelay anymore.");
    });
    
    config.AddCommand<ListInstancesCommand>("list-instances")
        .WithDescription("Lists the hosts and their keys");
    
    config.AddCommand<ConfigCommand>("config")
        .WithDescription("Initializes the FakeRelay configuration.");;
});

return app.Run(args);
