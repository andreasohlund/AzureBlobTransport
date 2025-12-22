using NServiceBus.Transport;

public class AzureBlobTransport(string connectionString) : TransportDefinition(TransportTransactionMode.ReceiveOnly, true, true, true)
{
    public override Task<TransportInfrastructure> Initialize(HostSettings hostSettings, ReceiveSettings[] receivers, string[] sendingAddresses, CancellationToken cancellationToken = new())
    {
        return Task.FromResult<TransportInfrastructure>(new AzureBlobTransportInfrastructure(receivers));
    }

    public override IReadOnlyCollection<TransportTransactionMode> GetSupportedTransactionModes() => [TransportTransactionMode.None, TransportTransactionMode.ReceiveOnly];
}