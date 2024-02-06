
namespace IoCloud.Shared.Storage.Documents.Abstractions
{
    public interface IDocumentStorage
    {
        Task<bool> ExistsAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> GetUrlAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken));
        Task<List<string>> GetUrlListAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task<List<string>> GetUrlListAsync(string prefix, CancellationToken cancellationToken = default(CancellationToken));
        Task DownloadAsync(string filePath, CancellationToken cancellationToken = default(CancellationToken));
        Task UploadAsync(string filePath, bool overwrite = false, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(string documentName, CancellationToken cancellationToken = default(CancellationToken));
    }
}