using FakeRelay.Cli.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<AddHostCommand>("add-host")
        .WithDescription("Adds a host to the relay and generates a key.")
        .WithExample(new[] {"mastodon.social"});
    
    config.AddCommand<UpdateHostCommand>("update-host");
    config.AddCommand<DeleteHostCommand>("delete-host");
    config.AddCommand<ConfigCommand>("config");
});

return app.Run(args);
