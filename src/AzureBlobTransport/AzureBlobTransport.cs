using NServiceBus.Transport;

public class AzureBlobTransport(string connectionString) : TransportDefinition(TransportTransactionMode.ReceiveOnly, true, true, true)
{
    public override Task<TransportInfrastructure> Initialize(HostSettings hostSettings, ReceiveSettings[] receivers, string[] sendingAddresses, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public override IReadOnlyCollection<TransportTransactionMode> GetSupportedTransactionModes() => throw new NotImplementedException();
}