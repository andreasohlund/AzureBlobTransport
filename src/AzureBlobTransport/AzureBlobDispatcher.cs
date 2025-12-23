using NServiceBus.Transport;

class AzureBlobDispatcher : IMessageDispatcher
{
    public Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
}