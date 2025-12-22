using NServiceBus.Transport;

class AzureBlobMessageReceiver(ReceiveSettings settings) : IMessageReceiver
{
    public Task Initialize(PushRuntimeSettings limitations, OnMessage onMessage, OnError onError, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task StartReceive(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task ChangeConcurrency(PushRuntimeSettings limitations, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task StopReceive(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public ISubscriptionManager Subscriptions { get; }
    public string Id { get; }
    public string ReceiveAddress { get; }
}