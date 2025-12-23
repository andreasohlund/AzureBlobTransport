using Azure.Storage.Blobs;
using NServiceBus.Transport;

class AzureBlobTransportInfrastructure : TransportInfrastructure
{
    public AzureBlobTransportInfrastructure(AzureBlobFolder blobFolder, ReceiveSettings[] receiverSettings)
    {
        var receivers = new Dictionary<string, IMessageReceiver>();
        foreach (var receiverSetting in receiverSettings)
        {
            receivers[receiverSetting.Id] = new AzureBlobMessageReceiver(receiverSetting, blobFolder, new AzureBlobSubscriptionManager());
        }

        Receivers = receivers;
        Dispatcher = new AzureBlobDispatcher();
    }

    public override Task Shutdown(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public override string ToTransportAddress(QueueAddress address) => address.BaseAddress;
}