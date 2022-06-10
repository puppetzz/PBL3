using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PBL3.Service {
    public class BlobService : IBlobService {
        private readonly BlobServiceClient _blobClient;
        public BlobService(BlobServiceClient blobClient) {
            _blobClient = blobClient;
        }

        public async Task<IEnumerable<string>> GetAllBlobsAsync(string containerName) {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);

            var files = new List<string>();

            var blobs = containerClient.GetBlobsAsync();

            await foreach (var item in blobs) {
                files.Add(item.Name);
            }

            return files;
        }

        public async Task<bool> DeleteBlobAsync(string name, string containerName) {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = containerClient.GetBlobClient(name);

            return await blobClient.DeleteIfExistsAsync();
        }

        public string GetBlob(string name, string containerName) {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);

            var blobClient = containerClient.GetBlobClient(name);

            return blobClient.Uri.AbsoluteUri;
        }

        public async Task<bool> UploadBlobAsync(string name, IFormFile file, string containerName) {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);
            var httpHeaders = new BlobHttpHeaders() {
                ContentType = file.ContentType
            };

            var res = await blobClient.UploadAsync(file.OpenReadStream(), httpHeaders);

            if (res != null)
                return true;

            return false;
        }
    }
}
