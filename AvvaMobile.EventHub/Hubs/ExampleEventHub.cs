using AvvaMobile.EventHubs.Abstractions;
using AvvaMobile.EventHubs.Constants;

namespace AvvaMobile.EventHubs.Hubs;

public class ExampleEventHub : IEventHubName
{
    public string HubName => HubNames.ExampleEvent;
    public string CheckpointStoreName => CheckpointStoreNames.ExampleEventCheckpointStore;
    public string PoisonMessagesBlobContainerName => PoisonQueueNames.ExampleEventPoisonQueue;
}