using FakeRelay.Cli.Settings;
using FakeRelay.Core;
using FakeRelay.Core.Helpers;
using Spectre.Console.Cli;

namespace FakeRelay.Cli.Commands;

public class ConfigCommand: Command<ConfigSettings>
{
    public override int Execute(CommandContext context, ConfigSettings settings)
    {
        var configPath = Environment.GetEnvironmentVariable("CONFIG_PATH");
        if (File.Exists(configPath))
        {
            throw new Exception("There's a config file");
        }

        var (pub, priv) = CryptographyHelper.GenerateKeys();
        Config.CreateConfig(configPath, settings.Host, pub, priv);
        
        return 0;
    }
}
