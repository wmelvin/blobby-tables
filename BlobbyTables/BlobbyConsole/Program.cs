using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobbyConsole;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static readonly string containerName = "blobby";
    private static readonly string blobName = "test.csv";

    private static async Task Main(string[] args)
    {
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

        if (settings.ConnectionString is null)
        {
            Console.WriteLine("ERROR: Cannot get 'ConnectionString' setting.");
            Environment.Exit(1);
        }

        try
        {
            await CreateContainerAndUploadAsync(settings);
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }


        static async Task CreateContainerAndUploadAsync(Settings settings)
        {
            Console.WriteLine("New BlobServiceClient.");
            BlobServiceClient serviceClient = new BlobServiceClient(settings.ConnectionString);

            Console.WriteLine("Get BlobContainerClient.");
            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(containerName);

            Console.WriteLine("Create BlobContainer.");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            Console.WriteLine("Get BlobClient.");
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            Console.WriteLine($"URI: {blobClient.Uri}");

            Console.WriteLine($"Upload {blobName}");
            using FileStream fs = File.OpenRead(blobName);

            await blobClient.UploadAsync(fs, new BlobHttpHeaders { ContentType = "text/csv" });

            Console.WriteLine("Uploaded.");
        }
    }
}