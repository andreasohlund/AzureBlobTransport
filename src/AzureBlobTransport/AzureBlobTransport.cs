using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NServiceBus.Transport;

public class AzureBlobTransport(string connectionString) : TransportDefinition(TransportTransactionMode.ReceiveOnly, true, true, true)
{
    public override async Task<TransportInfrastructure> Initialize(HostSettings hostSettings, ReceiveSettings[] receivers, string[] sendingAddresses, CancellationToken cancellationToken = new())
    {
        var containerClient = new BlobContainerClient(connectionString, serviceBusName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var rootPath = "/endpoints/";

        if (hostSettings.CoreSettings != null)
        {
            rootPath += hostSettings.CoreSettings.EndpointName();
        }
        else
        {
            rootPath += receivers.First().ReceiveAddress.BaseAddress;
        }

        var blobFolder = new AzureBlobFolder(containerClient, rootPath);
        var metadata = new Dictionary<string, string> { ["TransportVersion"] = "1.0" };

        await blobFolder.WriteJson(".metadata.json", metadata, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new AzureBlobTransportInfrastructure(blobFolder, receivers);
    }

    public override IReadOnlyCollection<TransportTransactionMode> GetSupportedTransactionModes() => [TransportTransactionMode.None, TransportTransactionMode.ReceiveOnly];

    const string serviceBusName = "nservicebus";
}

class AzureBlobFolder(BlobContainerClient containerClient, string rootPath)
{
    public async Task WriteJson<T>(
        string name,
        T value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = new())
    {
        var blob = containerClient.GetBlobClient(Path.Combine(rootPath, name));
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