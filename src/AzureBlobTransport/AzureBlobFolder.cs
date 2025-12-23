using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

class AzureBlobFolder(BlobContainerClient containerClient, string rootPath)
{
    public async Task WriteJson<T>(
        string name,
        T value,
        JsonSerializerOptions? options = null,
        Dictionary<string, string>? indexTags = null,
        CancellationToken cancellationToken = new())
    {
        var blob = containerClient.GetBlobClient(Path.Combine(rootPath, name));
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;

        var uploadOptions = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" }, Tags = indexTags };
        await blob.UploadAsync(
            stream,
            uploadOptions,
            cancellationToken
        ).ConfigureAwait(false);
    }
    
    public async Task Write(
        string name,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = new())
    {
        var blob = containerClient.GetBlobClient(Path.Combine(rootPath, name));
        
        using var stream = new MemoryStream(data.ToArray(), writable: false);
        await blob.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "application/octet-stream" } },
            cancellationToken
        ).ConfigureAwait(false);
    }

    public string RootPath => rootPath;
    public BlobContainerClient ContainerClient => containerClient;
}