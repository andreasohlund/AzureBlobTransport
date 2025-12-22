using Azure.Storage.Blobs;
using NServiceBus.Transport;

class AzureBlobMessageReceiver(BlobContainerClient containerClient, ReceiveSettings settings) : IMessageReceiver
{
    public async Task Initialize(PushRuntimeSettings limitations, OnMessage onMessage, OnError onError, CancellationToken cancellationToken = new())
    {
    }

    public Task StartReceive(CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public Task ChangeConcurrency(PushRuntimeSettings limitations, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task StopReceive(CancellationToken cancellationToken = new()) => throw new NotImplementedException();

    public ISubscriptionManager Subscriptions { get; }
    public string Id { get; }
    public string ReceiveAddress { get; }

    BlobContainerClient serviceBusContainer;
}