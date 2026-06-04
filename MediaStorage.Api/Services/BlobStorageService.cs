using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediaStorage.Api.Contracts;
using Microsoft.Extensions.Options;
using Azure.Storage.Sas;

namespace MediaStorage.Api.Services;

public class BlobStorageService(IOptions<BlobStorageOptions> options)
{
    private readonly BlobStorageOptions _options = options.Value;

    public async Task UploadAsync(Stream stream, string blobName, string contentType,CancellationToken cancellationToken)
    {
        var blobServiceClient = new BlobServiceClient(_options.ConnectionString);

        var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        await containerClient.CreateIfNotExistsAsync(PublicAccessType.None,cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            stream,
            new BlobHttpHeaders
            {
                ContentType = contentType
            },
            cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string blobName,CancellationToken cancellationToken)
    {
        var blobServiceClient = new BlobServiceClient(_options.ConnectionString);

        var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);

        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public string GenerateSasUrl(string blobName)
    {
        var blobServiceClient = new BlobServiceClient(_options.ConnectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(_options.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _options.ContainerName,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(8)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}