using FakeRelay.Core;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public abstract class ConfigEnabledAsyncCommand<T> : AsyncCommand<T>
    where T : CommandSettings
{
    protected ConfigEnabledAsyncCommand()
    {
        Config.Init(Environment.GetEnvironmentVariable("CONFIG_PATH"));
    }
}
