using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using NServiceBus.Transport;
using NServiceBus.TransportTests;

class ConfigureAzureBlobTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportDefinition CreateTransportDefinition() => new AzureBlobTransport(ConnectionString);

    public async Task<TransportInfrastructure> Configure(TransportDefinition transportDefinition, HostSettings hostSettings, QueueAddress inputQueue, string errorQueueName, CancellationToken cancellationToken = default)
    {
        var mainReceiverSettings = new ReceiveSettings(
            "mainReceiver",
            inputQueue,
            true,
            false,
            errorQueueName);

        var transport = await transportDefinition.Initialize(hostSettings, [mainReceiverSettings], [errorQueueName], cancellationToken);

        queuesToCleanUp = [transport.ToTransportAddress(inputQueue), errorQueueName];
        return transport;
    }

    public async Task Cleanup(CancellationToken cancellationToken = default)
    {
        foreach (var queue in queuesToCleanUp)
        {
            var containerClient = new BlobContainerClient(ConnectionString, "nservicebus");

            var blobClient = containerClient.GetBlobClient(Path.Combine("endpoints", queue));
            
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    string ConnectionString
    {
        get
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureBlobTransport_ConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("AzureBlobTransport_ConnectionString environment variable not set");
            }

            return connectionString;
        }
    }
    
    string[] queuesToCleanUp = [];
}