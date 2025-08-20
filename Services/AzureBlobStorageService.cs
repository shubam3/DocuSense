using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocuSense.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocuSense.Services
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var connectionString = _configuration["Azure:BlobStorage:ConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string? contentType = null)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobName = $"{Guid.NewGuid()}_{fileName}";
                var blobClient = containerClient.GetBlobClient(blobName);

                var blobHttpHeaders = new BlobHttpHeaders();
                if (!string.IsNullOrEmpty(contentType))
                {
                    blobHttpHeaders.ContentType = contentType;
                }

                await blobClient.UploadAsync(fileStream, blobHttpHeaders);

                _logger.Information("File uploaded successfully: {BlobName} to container {ContainerName}", blobName, containerName);
                return blobName;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error uploading file {FileName} to container {ContainerName}", fileName, containerName);
                throw;
            }
        }

        public async Task<Stream> DownloadFileAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"Blob {blobName} not found in container {containerName}");
                }

                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error downloading blob {BlobName} from container {ContainerName}", blobName, containerName);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DeleteIfExistsAsync();
                
                _logger.Information("File deleted successfully: {BlobName} from container {ContainerName}", blobName, containerName);
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting blob {BlobName} from container {ContainerName}", blobName, containerName);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if blob {BlobName} exists in container {ContainerName}", blobName, containerName);
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string blobName, string containerName, TimeSpan? expiry = null)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                if (expiry.HasValue)
                {
                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = containerName,
                        BlobName = blobName,
                        Resource = "b",
                        StartsOn = DateTimeOffset.UtcNow,
                        ExpiresOn = DateTimeOffset.UtcNow.Add(expiry.Value)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);

                    var sasToken = sasBuilder.ToSasQueryParameters(new Azure.Storage.StorageSharedKeyCredential(
                        _blobServiceClient.AccountName, 
                        _configuration["Azure:BlobStorage:AccountKey"])).ToString();

                    return $"{blobClient.Uri}?{sasToken}";
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting URL for blob {BlobName} in container {ContainerName}", blobName, containerName);
                throw;
            }
        }

        public async Task<long> GetFileSizeAsync(string blobName, string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var properties = await blobClient.GetPropertiesAsync();
                return properties.Value.ContentLength;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting file size for blob {BlobName} in container {ContainerName}", blobName, containerName);
                throw;
            }
        }

        public async Task<List<string>> ListFilesAsync(string containerName, string? prefix = null)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var files = new List<string>();

                await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    files.Add(blobItem.Name);
                }

                return files;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error listing files in container {ContainerName}", containerName);
                throw;
            }
        }

        public async Task<bool> CreateContainerAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.CreateIfNotExistsAsync();
                
                _logger.Information("Container created successfully: {ContainerName}", containerName);
                return response != null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating container {ContainerName}", containerName);
                throw;
            }
        }

        public async Task<bool> DeleteContainerAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var response = await containerClient.DeleteIfExistsAsync();
                
                _logger.Information("Container deleted successfully: {ContainerName}", containerName);
                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting container {ContainerName}", containerName);
                throw;
            }
        }

        public async Task<bool> ContainerExistsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                return await containerClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error checking if container {ContainerName} exists", containerName);
                return false;
            }
        }
    }
} 