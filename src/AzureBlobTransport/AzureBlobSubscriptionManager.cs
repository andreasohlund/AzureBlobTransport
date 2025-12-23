using NServiceBus.Extensibility;
using NServiceBus.Transport;
using NServiceBus.Unicast.Messages;

class AzureBlobSubscriptionManager : ISubscriptionManager
{
    public Task SubscribeAll(MessageMetadata[] eventTypes, ContextBag context, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task Unsubscribe(MessageMetadata eventType, ContextBag context, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
}