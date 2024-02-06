using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Storage.Documents.Abstractions;

namespace IoCloud.Shared.Storage.Blobs.Infrastructure
{
    /// <summary>
    /// Implements IDocumentStorage for getting the url/uploading/downloading/deleteing a file from blob storage
    /// </summary>
    public class BlobStorage : IDocumentStorage
    {
        private readonly IBlobStorageConfiguration _configuration;
        private readonly BlobServiceClient _client;
        private readonly ILogger<BlobStorage> _logger;

        public BlobStorage
        (
            IBlobStorageConfiguration configuration,
            BlobServiceClient client,
            ILogger<BlobStorage> logger
        )
        {
            _configuration = configuration;
            _client = client;
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = await GetBlobContainer();
                var blockBlob = container.GetBlobClient(documentName);
                var doesBlobExist = await blockBlob.ExistsAsync(cancellationToken);
                return doesBlobExist.Value;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Blob {documentName} existence cannot be verified - error details: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetUrlAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = await GetBlobContainer();
                var blockBlob = container.GetBlobClient(documentName);

                var exists = await blockBlob.ExistsAsync(cancellationToken);

                string blobUrl = exists ? blockBlob.Uri.ToString() : string.Empty;
                return blobUrl;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Url for blob {documentName} was not found - error details: {ex.Message}");
                throw;
            }
        }

        public async Task<List<string>> GetUrlListAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await GetUrlListAsync(null, cancellationToken);

        public async Task<List<string>> GetUrlListAsync(string prefix, CancellationToken cancellationToken = default(CancellationToken))
        {
            var container = await GetBlobContainer();

            // Create a new list object for 
            var files = new List<string>();

            try
            {
                await foreach (BlobItem file in container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix, cancellationToken))
                {
                    // Add each file retrieved from the storage container to the files list by creating a BlobDto object
                    string uri = container.Uri.ToString();
                    var name = file.Name;
                    var fullUri = $"{uri}/{name}";

                    files.Add(fullUri);
                }
                return files;
            }
            catch (RequestFailedException ex)
            {
                var msg = string.IsNullOrEmpty(prefix) ? "" : $" with prefix {prefix}";
                _logger.LogError($"Urls for container {container.Name}{msg} was not found - error details: {ex.Message}");
                throw;
            }
        }

        public async Task DownloadAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            string blobName = "";
            try
            {
                var container = await GetBlobContainer();
                blobName = Path.GetFileName(filePath);
                var blockBlob = container.GetBlobClient(blobName);

                if (await blockBlob.ExistsAsync(cancellationToken))
                { 
                    await blockBlob.DownloadToAsync(filePath, cancellationToken);
                }
            }

            catch (RequestFailedException ex)
            {
                _logger.LogError($"Cannot download blob {blobName} - error details: {ex.Message}");
                throw;
            }
        }

        public async Task UploadAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            string blobName = "";
            try
            {
                var container = await GetBlobContainer();
                blobName = Path.GetFileName(filePath);
                BlobClient blob = container.GetBlobClient(blobName);
                await blob.UploadAsync(filePath, overwrite, cancellationToken);
            }

            catch (RequestFailedException ex)
            {
                _logger.LogError($"Blob {blobName} was not uploaded successfully - error details: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var container = await GetBlobContainer();
                var blockBlob = container.GetBlobClient(documentName);
                await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, cancellationToken);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Blob {documentName} was not deleted successfully - error details: {ex.Message}");
                throw;
            }
        }

        private async Task<BlobContainerClient> GetBlobContainer()
        {
            try
            {
                BlobContainerClient container = _client
                                .GetBlobContainerClient(_configuration.ContainerName);

                await container.CreateIfNotExistsAsync();

                return container;
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError($"Cannot find blob container: {_configuration.ContainerName} - error details: {ex.Message}");
                throw;
            }
        }
    }
}