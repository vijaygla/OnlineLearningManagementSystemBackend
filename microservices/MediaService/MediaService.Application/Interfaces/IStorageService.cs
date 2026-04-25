using System.IO;
using System.Threading.Tasks;

namespace MediaService.Application.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Uploads a file to the storage provider.
        /// </summary>
        /// <param name="fileStream">The stream of the file to upload.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="contentType">The content type of the file (e.g., video/mp4).</param>
        /// <returns>The URL or path of the uploaded file.</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);

        /// <summary>
        /// Deletes a file from the storage provider.
        /// </summary>
        /// <param name="fileName">The name/path of the file to delete.</param>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// Gets a temporary public URL for a file (useful for private buckets).
        /// </summary>
        Task<string> GetFileUrlAsync(string fileName);
    }
}
