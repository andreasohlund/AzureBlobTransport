using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

class AzureBlobMessageReceiver(ReceiveSettings receiveSettings, BlobContainerClient containerClient, AzureBlobFolder bodyFolder, AzureBlobSubscriptionManager subscriptionManager) : IMessageReceiver
{
    public async Task Initialize(PushRuntimeSettings limitations, OnMessage onMessage, OnError onError, CancellationToken cancellationToken = new())
    {
        onMessageReceived = onMessage;
        onMessageFailed = onError;
    }

    public async Task StartReceive(CancellationToken cancellationToken = new())
    {
        messageReceivingCancellationTokenSource = new CancellationTokenSource();
        messageProcessingCancellationTokenSource = new CancellationTokenSource();

        messageReceivingTask =
            Task.Run(() => ReceiveMessagesAndSwallowExceptions(messageReceivingCancellationTokenSource.Token),
                CancellationToken.None);
    }

    async Task ReceiveMessagesAndSwallowExceptions(CancellationToken messageReceivingCancellationToken)
    {
        var query = $"state = 'available' AND endpoint = '{receiveSettings.ReceiveAddress.BaseAddress}'";
        while (!messageReceivingCancellationToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var blob in containerClient.FindBlobsByTagsAsync(query, cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false))
                {
                    await ProcessMessage(blob, messageReceivingCancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // private token, receiver is being stopped, log the exception in case the stack trace is ever needed for debugging
                break;
            }
        }
    }

    async Task ProcessMessage(TaggedBlobItem blob, CancellationToken cancellationToken)
    {
        var nativeMessageId = Path.GetFileNameWithoutExtension(blob.BlobName);
        var messageClient = containerClient.GetBlobClient(blob.BlobName);
        var leaseClient = messageClient.GetBlobLeaseClient();
        try
        {
            var lease = await leaseClient.AcquireAsync(TimeSpan.FromSeconds(30), cancellationToken: cancellationToken).ConfigureAwait(false);
            var tagResponse = await messageClient.GetTagsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            var tags = tagResponse.Value.Tags;

            if (tags["state"] != "available")
            {
                return;
            }

            tags["state"] = "processed";
            tags["processed-at"] = DateTime.UtcNow.ToString("O");

            var messageResult = await messageClient.DownloadStreamingAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(messageResult.Value.Content);

            var bodyBlob = await bodyFolder.Read(nativeMessageId, cancellationToken).ConfigureAwait(false);

            var context = new MessageContext(blob.BlobName, headers, bodyBlob.Content.ToMemory(), new TransportTransaction(), receiveSettings.ReceiveAddress.BaseAddress, new ContextBag());
            await onMessageReceived(context, cancellationToken: cancellationToken).ConfigureAwait(false);

            await messageClient.SetTagsAsync(tags, conditions: new BlobRequestConditions { LeaseId = lease.Value.LeaseId }, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (leaseClient != null)
            {
                await leaseClient.ReleaseAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public Task ChangeConcurrency(PushRuntimeSettings limitations, CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public Task StopReceive(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public ISubscriptionManager Subscriptions => subscriptionManager;

    public string Id => receiveSettings.Id;
    public string ReceiveAddress => receiveSettings.ReceiveAddress.BaseAddress;

    CancellationTokenSource messageReceivingCancellationTokenSource;
    CancellationTokenSource messageProcessingCancellationTokenSource;
    Task messageReceivingTask;
    OnMessage onMessageReceived;
    OnError onMessageFailed;
}