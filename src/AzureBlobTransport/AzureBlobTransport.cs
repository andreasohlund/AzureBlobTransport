using Azure.Storage.Blobs;
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

        var endpointFolder = new AzureBlobFolder(containerClient, rootPath);
        var metadata = new Dictionary<string, string> { ["TransportVersion"] = "1.0" };

        await endpointFolder.WriteJson(".metadata.json", metadata, cancellationToken: cancellationToken).ConfigureAwait(false);

        
        var bodyFolder = new AzureBlobFolder(containerClient, "/bodies");
        return new AzureBlobTransportInfrastructure(bodyFolder, receivers);
    }

    public override IReadOnlyCollection<TransportTransactionMode> GetSupportedTransactionModes() => [TransportTransactionMode.None, TransportTransactionMode.ReceiveOnly];

    const string serviceBusName = "nservicebus";
}