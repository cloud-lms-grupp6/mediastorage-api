using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediaStorage.Api.Contracts;
using Microsoft.Extensions.Options;

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
}