using NServiceBus.Transport;

class AzureBlobMessageReceiver(ReceiveSettings receiveSettings, AzureBlobFolder blobFolder, AzureBlobSubscriptionManager subscriptionManager) : IMessageReceiver
{
    public async Task Initialize(PushRuntimeSettings limitations, OnMessage onMessage, OnError onError, CancellationToken cancellationToken = new())
    {
    }

    public Task StartReceive(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task ChangeConcurrency(PushRuntimeSettings limitations, CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task StopReceive(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public ISubscriptionManager Subscriptions => subscriptionManager;

    public string Id => receiveSettings.Id;
    public string ReceiveAddress => blobFolder.RootPath;
}