using Azure;
using Azure.Data.Tables;

namespace BlobbyConsole
{
    internal class UploadsTableEntity : ITableEntity
    {
        public string FileName { get; set; }
        public DateTime WhenUploaded { get; set; }

        // Required to implement ITableEntity:
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
