namespace DocuSense.Services.Interfaces
{
    public interface IAzureBlobStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string? contentType = null);
        Task<Stream> DownloadFileAsync(string blobName, string containerName);
        Task<bool> DeleteFileAsync(string blobName, string containerName);
        Task<bool> FileExistsAsync(string blobName, string containerName);
        Task<string> GetFileUrlAsync(string blobName, string containerName, TimeSpan? expiry = null);
        Task<long> GetFileSizeAsync(string blobName, string containerName);
        Task<List<string>> ListFilesAsync(string containerName, string? prefix = null);
        Task<bool> CreateContainerAsync(string containerName);
        Task<bool> DeleteContainerAsync(string containerName);
        Task<bool> ContainerExistsAsync(string containerName);
    }
} 