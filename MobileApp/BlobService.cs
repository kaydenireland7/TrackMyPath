using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using static System.Net.WebRequestMethods;

namespace MobileApp
{
    class BlobService
    {


        private BlobServiceClient blobServiceClient;
        private BlobContainerClient containerClient;
        private string localFilePath;

        public BlobService()
        {

            blobServiceClient = new BlobServiceClient("");
            containerClient = blobServiceClient.GetBlobContainerClient("images");

        }


        public void SetLocalFilePath(string lfp)
        {
            localFilePath = lfp;
        }


        public async Task<int> FindNumberOfBlobs()
        {
            int count = 0;
            await foreach (BlobItem blobItem in this.containerClient.GetBlobsAsync())
            {
                count++;
            }
            return count;
        }


        public async Task ListBlobsInContainerAsync()
        {
            await foreach (BlobItem blobItem in this.containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"- {blobItem.Name}");
               
            }
        }

        public async Task<string[]> FindArrayOfBlobs()
        {
            int count = await FindNumberOfBlobs();
            string[] blobs = new string[count];
            int i = 0;
            await foreach (BlobItem blobItem in this.containerClient.GetBlobsAsync())
            {
                blobs[i] = blobItem.Name;
                i++;
            }
            return blobs;
        }

        public async Task UploadBlobAsync(string blobName)
        {
            BlobClient blobClient = this.containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Uploading to Blob storage: {blobClient.Uri}");

            await blobClient.UploadAsync(localFilePath, true);
        }

        public async Task DownloadBlobAsync(string blobName, string downloadFilePath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Downloading blob to {downloadFilePath}");

            await blobClient.DownloadToAsync(downloadFilePath);
        }

        public async Task DeleteBlobAsync(string blobName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Deleting blob: {blobClient.Uri}");

            await blobClient.DeleteIfExistsAsync();
        }


    }
}
