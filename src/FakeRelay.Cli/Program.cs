using FakeRelay.Cli.Commands;
using FakeRelay.Cli.Settings;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddBranch<EmptyBaseSettings>("instance", instance =>
    {
        instance.AddCommand<AddInstanceCommand>("add")
            .WithDescription("Adds an instance to the relay and generates a key.");
        instance.AddCommand<UpdateInstanceCommand>("update")
            .WithDescription("Generates a new key for the instance. The old one can't be used anymore.");
        instance.AddCommand<DeleteInstanceCommand>("delete")
            .WithDescription("Deletes the existing keys for the instance. They can't use FakeRelay anymore.");
    });
    
    config.AddCommand<ListInstancesCommand>("list-instances")
        .WithDescription("Lists the hosts and their keys");
    
    config.AddCommand<ConfigCommand>("config")
        .WithDescription("Initializes the FakeRelay configuration.");;
});

return app.Run(args);
