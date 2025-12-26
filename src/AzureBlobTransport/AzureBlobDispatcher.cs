using NServiceBus.Transport;

class AzureBlobDispatcher(AzureBlobFolder bodyFolder) : IMessageDispatcher
{
    public async Task Dispatch(TransportOperations outgoingMessages, TransportTransaction transaction, CancellationToken cancellationToken = new())
    {
        // TODO: task when all
        // TODO: write all bodies first
        foreach (var unicastOperation in outgoingMessages.UnicastTransportOperations)
        {
            var nativeMessageId = Guid.NewGuid().ToString();
            
            await bodyFolder.Write(nativeMessageId, unicastOperation.Message.Body, cancellationToken).ConfigureAwait(false);

            var targetFolder = new AzureBlobFolder(bodyFolder.ContainerClient, Path.Combine("endpoints", unicastOperation.Destination));

            var tags = new Dictionary<string, string>
            {
                { "state", "available" },
                { "endpoint", unicastOperation.Destination }
            };

            await targetFolder.WriteJson(nativeMessageId + ".json", unicastOperation.Message.Headers, indexTags: tags, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}