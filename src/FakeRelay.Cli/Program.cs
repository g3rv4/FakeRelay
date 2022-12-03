using FakeRelay.Cli.Commands;
using FakeRelay.Core;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(config =>
{
    config.AddCommand<AddHostCommand>("add-host");
    config.AddCommand<UpdateHostCommand>("update-host");
    config.AddCommand<DeleteHostCommand>("delete-host");
});

Config.Init(Environment.GetEnvironmentVariable("CONFIG_PATH"));

return app.Run(args);