using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Options;
using AvvaMobile.EventHubs.Abstractions;
using AvvaMobile.EventHubs.Configurations;

namespace Mlink.AdDistribution.Shared.EventHubs.Implementations;

public class EventHubConsumer<THub> : IEventHubConsumer<THub>
    where THub: IEventHubName
{
    private readonly EventHubConsumerClient _eventHubConsumerClient;
    
    public EventHubConsumer(IOptions<EventHubOptions> settings, THub hubConfig)
    {
        _eventHubConsumerClient = new EventHubConsumerClient(
            EventHubConsumerClient.DefaultConsumerGroupName, 
            settings.Value.HubConnection, 
            hubConfig.HubName);
    }
    
    public IAsyncEnumerable<PartitionEvent> ReadEventsAsync(string partitionId, ReadEventOptions options, CancellationToken stoppingToken)
    {
        var eventPosition = EventPosition.Latest;

        return _eventHubConsumerClient.ReadEventsFromPartitionAsync(partitionId, eventPosition, options, stoppingToken);
    }
    
    public Task<string[]> GetPartitionIdsAsync(CancellationToken stoppingToken)
    {
        return _eventHubConsumerClient.GetPartitionIdsAsync(stoppingToken);
    }
}