using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

static class BlobClientExtensions
{
    public static async Task UploadJsonAsync<T>(
        this BlobClient blob,
        T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken).ConfigureAwait(false);
        stream.Position = 0;

        await blob.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "application/json" } },
            cancellationToken
        ).ConfigureAwait(false);
    }
}