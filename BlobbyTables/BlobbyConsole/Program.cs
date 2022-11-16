using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobbyConsole;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static readonly string _containerName = "blobby";
    private static readonly string _blobName = "test.csv";

    private static async Task Main(string[] args)
    {
        Settings settings = GetSettings();
        try
        {
            await CreateContainerAndUploadAsync(settings);
            
            PromptToContinue("list contents");

            await ListContainerContentsAsync(settings);
            
            PromptToContinue("download");

            await DownloadTheBlobAsync(settings);
            
            PromptToContinue("delete container");

            await DeleteContainerAsync(settings);

            PromptToContinue("finish");
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
    }

    private static Settings GetSettings()
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
        
        return settings;
    }

    private static void PromptToContinue(string promptTo = "run next step" )
    {
        Console.WriteLine();
        Console.Write($"Press [Enter] to {promptTo}. ");
        Console.ReadLine();
        Console.WriteLine();
    }

    private static async Task CreateContainerAndUploadAsync(Settings settings)
    {
        Console.WriteLine("New BlobServiceClient.");
        BlobServiceClient serviceClient = new BlobServiceClient(settings.ConnectionString);

        Console.WriteLine("Get BlobContainerClient.");
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(_containerName);

        Console.WriteLine("Create BlobContainer.");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

        Console.WriteLine("Get BlobClient.");
        BlobClient blobClient = containerClient.GetBlobClient(_blobName);

        Console.WriteLine($"URI: {blobClient.Uri}");

        Console.WriteLine($"Upload {_blobName}");
        using FileStream fs = File.OpenRead(_blobName);

        await blobClient.UploadAsync(fs, new BlobHttpHeaders { ContentType = "text/csv" });

        Console.WriteLine("Set metadata.");

        IDictionary<string, string> metadata = new Dictionary<string, string>();
        metadata["user_name"] = "heydude";
        metadata["user_org"] = "dontcallmedude";
        await blobClient.SetMetadataAsync(metadata);

        Console.WriteLine("List properties.");

        BlobProperties bp = await blobClient.GetPropertiesAsync();
        Console.WriteLine($"          ETag: {bp.ETag}");
        Console.WriteLine($"  LastModified: {bp.LastModified}");
        Console.WriteLine($"   ContentType: {bp.ContentType}");
        Console.WriteLine($"      BlobType: {bp.BlobType}");
        foreach (var item in bp.Metadata)
        {
            Console.WriteLine($"      Metadata: {item.Key} = '{item.Value}'");
        }
    }

    private static async Task ListContainerContentsAsync(Settings settings)
    {
        Console.WriteLine("New BlobServiceClient.");
        BlobServiceClient serviceClient = new BlobServiceClient(settings.ConnectionString);

        Console.WriteLine("List containers:");
        await foreach (BlobContainerItem container in serviceClient.GetBlobContainersAsync())
        {
            Console.WriteLine($"  Container: {container.Name}");

            BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(container.Name);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"    Blob: {blob.Name}");
                Console.WriteLine($"          ETag: {blob.Properties.ETag}");
                Console.WriteLine($"  LastModified: {blob.Properties.LastModified}");
                Console.WriteLine($"   ContentType: {blob.Properties.ContentType}");
                Console.WriteLine($"      BlobType: {blob.Properties.BlobType}");

                Console.WriteLine("Metadata from BlobItem:");
                foreach (var item in blob.Metadata)
                {
                    Console.WriteLine($"  Metadata: {item.Key} = '{item.Value}'");
                }

                // It appears that even though the BlobItem has a Metadata property
                // it is not the same as that from the BlobClient's properties.

                Console.WriteLine("Metadata from BlobClient.GetPropertiesAsync:");
                BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
                BlobProperties bp = await blobClient.GetPropertiesAsync();
                foreach (var item in bp.Metadata)
                {
                    Console.WriteLine($"  Metadata: '{item.Key}'='{item.Value}'");
                }
            }
        }
    }

    private static async Task DownloadTheBlobAsync(Settings settings)
    {
        Console.WriteLine("New BlobServiceClient.");
        BlobServiceClient serviceClient = new BlobServiceClient(settings.ConnectionString);

        Console.WriteLine("Get BlobContainerClient.");
        BlobContainerClient containerClient = serviceClient.GetBlobContainerClient(_containerName);

        Console.WriteLine("Get BlobClient.");
        BlobClient blobClient = containerClient.GetBlobClient(_blobName);

        Console.WriteLine("Download blob.");
        bool blobExists = await blobClient.ExistsAsync();
        if (blobExists)
        {
            string fileName = "C:\\Temp\\download-test.csv";
            BlobDownloadInfo info = await blobClient.DownloadAsync();

            Console.WriteLine($"Save as '{fileName}'.");

            using FileStream fs = File.OpenWrite(fileName);
            await info.Content.CopyToAsync(fs);
        }
    }

    private static async Task DeleteContainerAsync(Settings settings)
    {
        Console.WriteLine("New BlobContainerClient.");
        BlobContainerClient containerClient = new BlobContainerClient(
            settings.ConnectionString, _containerName);

        Console.WriteLine($"Delete container '{containerClient.Name}'.");

        await containerClient.DeleteIfExistsAsync();
    }

}