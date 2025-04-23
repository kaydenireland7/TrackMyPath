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

        public BlobService(string lfp)
        {

            blobServiceClient = new BlobServiceClient("");
            containerClient = blobServiceClient.GetBlobContainerClient("images");

            localFilePath = lfp;

        }

        public void setLocalFilePath(string lfp)
        {
            localFilePath = lfp;
        }


        public static async Task ListBlobsInContainerAsync(BlobContainerClient containerClient)
        {
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"- {blobItem.Name}");
            }
        }

        public static async Task UploadBlobAsync(BlobContainerClient containerClient, string blobName, string localFilePath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Uploading to Blob storage: {blobClient.Uri}");

            await blobClient.UploadAsync(localFilePath, true);
        }

        public static async Task DownloadBlobAsync(BlobContainerClient containerClient, string blobName, string downloadFilePath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Downloading blob to {downloadFilePath}");

            await blobClient.DownloadToAsync(downloadFilePath);
        }

        public static async Task DeleteBlobAsync(BlobContainerClient containerClient, string blobName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(blobName);
            Console.WriteLine($"Deleting blob: {blobClient.Uri}");

            await blobClient.DeleteIfExistsAsync();
        }


    }
}
