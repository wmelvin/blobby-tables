using BlobbyConsole;
using Microsoft.Extensions.Configuration;

string configFile = Path.Join(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "KeepLocal", "blobby-config.json");

if (!File.Exists(configFile))
{
    Console.WriteLine($"ERROR: Cannot find {configFile}");
    Environment.Exit(1);
}

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile(configFile)
    .AddEnvironmentVariables()
    .Build();

Settings? settings = config.GetRequiredSection("Settings").Get<Settings>();
if (settings is null)
{
    Console.WriteLine($"ERROR: Cannot get Settings from {configFile}");
    Environment.Exit(1);
}

if (settings.ContainerKey is null)
{
    Console.WriteLine("ERROR: Cannot get 'ContainerKey' setting.");
    Environment.Exit(1);
}

Console.WriteLine($"ContainerKey='{settings.ContainerKey}'");
