using System.Text.Json;
using NServiceBus.Extensibility;
using NServiceBus.Transport;

class AzureBlobMessageReceiver(ReceiveSettings receiveSettings, AzureBlobFolder bodyFolder, AzureBlobSubscriptionManager subscriptionManager) : IMessageReceiver
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
        var containerClient = bodyFolder.ContainerClient;
        var query = $"state = 'commited' AND endpoint = '{receiveSettings.ReceiveAddress.BaseAddress}'";
        while (!messageReceivingCancellationToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var blob in containerClient.FindBlobsByTagsAsync(query, cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false))
                {
                    var nativeMessageId = Path.GetFileNameWithoutExtension(blob.BlobName);
                    var blobClient = containerClient.GetBlobClient(blob.BlobName);
                    var headerPayload = await blobClient.DownloadStreamingAsync(cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false);
                    var headers = JsonSerializer.Deserialize<Dictionary<string, string>>(headerPayload.Value.Content);
                    
                    var bodyBlob = await bodyFolder.Read(nativeMessageId, messageReceivingCancellationToken).ConfigureAwait(false);
                    
                    var context = new MessageContext(blob.BlobName, headers, bodyBlob.Content.ToMemory(), new TransportTransaction(), receiveSettings.ReceiveAddress.BaseAddress, new ContextBag());
                    await onMessageReceived(context, cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false);

                    var tagResponse = await blobClient.GetTagsAsync(cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false);
                    var tags = tagResponse.Value.Tags;

                    tags["state"] = "processed";

                    await blobClient.SetTagsAsync(tags, cancellationToken: messageReceivingCancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // private token, receiver is being stopped, log the exception in case the stack trace is ever needed for debugging
                break;
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