
internal class Program
{

    // TODO: Figure out the right way to read configuration, including locally
    // stored secrets that are outside the project source tree, in a C#
    // Console app. Configuration seems to be a big topic in .NET with lots
    // of interfaces, classes, extension methods, providers, and such; and
    // much of the information in the docs and blogs is about configuring
    // web apps, not console apps. I couldn't get UserSecrets working here.
    //
    // For now, this...

    private static Dictionary<string, string> LocalConfig()
    {
        var settings = new Dictionary<string, string>();

        var fileName = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "KeepLocal", "blobby-config.txt"
        );
        // TODO: Would want to make sure the file exists it this was a keeper,
        // but it's just a temporary demo thing.

        char[] trimmy= { ' ', '"', '\'' };
        var reader = new StreamReader(fileName);
        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (!string.IsNullOrEmpty(line) && line.Contains('='))
            {
                string[] a = line.Split("=");
                // TODO: Is there a slicing syntax to take everything after
                // the first '=' in the case of a setting with a '=' in the
                // value (like a connection string)? The length will be > 2.
                if (a.Length == 2)
                {
                    settings[a[0].Trim()] = a[1].Trim(trimmy);
                }
            }
        }
        reader.Close();
        return settings;
    }
    private static void Main(string[] args)
    {
        var localConfig = LocalConfig();
        localConfig.TryGetValue("ContainerKey", out string? containerKey);
        Console.WriteLine($"ContainerKey=\"{containerKey}\"");

        Console.WriteLine("Where's the BLOB?");
    }
}