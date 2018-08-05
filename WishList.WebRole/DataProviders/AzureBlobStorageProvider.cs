using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WishList.WebRole.DataProviders
{
    /// <summary>
    /// Class definition for the azure blob storage provider
    /// </summary>
    public class AzureBlobStorageProvider
    {
        /// <summary>
        /// Storage account connect to Azure storage
        /// </summary>
        private CloudStorageAccount storageAccount;

        /// <summary>
        /// Default constructor
        /// </summary>
        public AzureBlobStorageProvider()
        {
        }

        /// <summary>
        /// Constructor of an Azure Blob storage provider
        /// </summary>
        /// <param name="connectionString">connection string</param>
        public AzureBlobStorageProvider(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        /// <summary>
        /// Constructor of an Azure Blob storage provider
        /// </summary>
        /// <param name="account">A settled Azure cloud storage account</param>
        public AzureBlobStorageProvider(CloudStorageAccount account)
        {
            this.storageAccount = account;
        }

        /// <summary>
        /// Save some data to blob storage
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <param name="key">data key</param>
        /// <param name="stream">data to store</param>
        /// <returns>Task to return</returns>
        public async Task SaveBlobDataAsync(string containerName, string key, Stream stream)
        {
            // Create the blob client.
            CloudBlobClient blobClient = this.storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = container.GetBlockBlobReference(key);
            await blob.UploadFromStreamAsync(stream);
        }

        /// <summary>
        /// Get the contents of the blob for the given key
        /// </summary>
        /// <param name="containerName">Container name</param>
        /// <param name="key">The blob key</param>
        /// <returns>blob data</returns>
        public async Task<Stream> GetBlobDataAsync(string containerName, string key)
        {
            // Create the blob client.
            CloudBlobClient blobClient = this.storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = container.GetBlockBlobReference(key);
            Stream stream = new MemoryStream();
            await blob.DownloadToStreamAsync(stream);

            return stream;
        }

        /// <summary>
        /// Judge the existence of a key in blob
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <param name="key">The blob key</param>
        /// <returns>Boolean shows blob exist or not</returns>
        public async Task<bool> ExistsAsync(string containerName, string key)
        {
            // Create the blob client.
            CloudBlobClient blobClient = this.storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            return await container.GetBlockBlobReference(key).ExistsAsync();
        }

        /// <summary>
        /// Delete some data from blob storage
        /// </summary>
        /// <param name="containerName">container name</param>
        /// <param name="key">data key</param>
        /// <returns>Task to return</returns>
        public async Task DeleteBlobDataAsync(string containerName, string key)
        {
            // Create the blob client.
            CloudBlobClient blobClient = this.storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            CloudBlockBlob blob = container.GetBlockBlobReference(key);
            await blob.DeleteIfExistsAsync();
        }
    }
}
