using Azure.Messaging.EventHubs.Consumer;

namespace AvvaMobile.EventHubs.Abstractions;

public interface IEventHubConsumer<THub> where THub : IEventHubName
{
	IAsyncEnumerable<PartitionEvent> ReadEventsAsync(string partitionId, ReadEventOptions options, CancellationToken stoppingToken);
	Task<string[]> GetPartitionIdsAsync(CancellationToken stoppingToken);
}