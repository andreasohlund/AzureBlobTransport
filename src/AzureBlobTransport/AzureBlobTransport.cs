using Azure.Storage.Blobs;
using NServiceBus.Transport;

public class AzureBlobTransport(string connectionString) : TransportDefinition(TransportTransactionMode.ReceiveOnly, true, true, true)
{
    public override async Task<TransportInfrastructure> Initialize(HostSettings hostSettings, ReceiveSettings[] receivers, string[] sendingAddresses, CancellationToken cancellationToken = new())
    {
        var containerClient = new BlobContainerClient(connectionString, serviceBusName);

        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

        var endpointName = receivers.First().ReceiveAddress.BaseAddress;
        if (hostSettings.CoreSettings != null)
        {
            endpointName = hostSettings.CoreSettings.EndpointName();
        }

        var endpointClient = containerClient.GetBlobClient(endpointName + "/.metadata.json");
        var metadata = new Dictionary<string, string>
        {
            ["TransportVersion"] = "1.0"
        };

        await endpointClient.UploadJsonAsync(metadata, cancellationToken: cancellationToken).ConfigureAwait(false);

        return new AzureBlobTransportInfrastructure(containerClient, endpointName, receivers);
    }

    public override IReadOnlyCollection<TransportTransactionMode> GetSupportedTransactionModes() => [TransportTransactionMode.None, TransportTransactionMode.ReceiveOnly];

    const string serviceBusName = "my-service-bus";
}