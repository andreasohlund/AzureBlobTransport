using Azure.Storage.Blobs;
using NServiceBus.Transport;

class AzureBlobMessageReceiver(AzureBlobFolder blobFolder) : IMessageReceiver
{
    public async Task Initialize(PushRuntimeSettings limitations, OnMessage onMessage, OnError onError, CancellationToken cancellationToken = new())
    {
    }

    public Task StartReceive(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task ChangeConcurrency(PushRuntimeSettings limitations, CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task StopReceive(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public ISubscriptionManager Subscriptions { get; }
    public string Id { get; }
    public string ReceiveAddress { get; }

    BlobContainerClient serviceBusContainer;
}