using Azure.Messaging.EventHubs;

namespace AvvaMobile.EventHubs.Abstractions;

public interface IEventHubProcessor<THub> where THub : IEventHubName
{
	EventProcessorClient Client { get; }

	Task MoveToPoisonQueue(EventData eventData, Exception ex);
}