using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Transport;
using NServiceBus.TransportTests;

class ConfigureAzureBlobTransportInfrastructure : IConfigureTransportInfrastructure
{
    public TransportDefinition CreateTransportDefinition()
    {
        var connectionString = Environment.GetEnvironmentVariable("AzureBlobTransport_ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("AzureBlobTransport_ConnectionString environment variable not set");
        }

        return new AzureBlobTransport(connectionString);
    }

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

    public Task Cleanup(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    string[] queuesToCleanUp;
}