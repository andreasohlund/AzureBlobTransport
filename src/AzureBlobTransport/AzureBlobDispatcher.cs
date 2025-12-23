using NServiceBus.Transport;

class AzureBlobDispatcher(AzureBlobFolder bodyFolder) : IMessageDispatcher
{
    public async Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, CancellationToken cancellationToken = new())
    {
        // TODO: task when all
        foreach (var unicastOperation in outgoingMessages.UnicastTransportOperations)
        {
            var bodyId = Guid.NewGuid().ToString();
            await bodyFolder.Write(bodyId, unicastOperation.Message.Body, cancellationToken).ConfigureAwait(false);
        }
    }
}