using AvvaMobile.EventHubs.Abstractions;
using AvvaMobile.EventHubs.Configurations;
using AvvaMobile.EventHubs.Models;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace AvvaMobile.EventHubs.Implementations;

public class EventHubProcessor<THub> : IEventHubProcessor<THub>
	where THub : IEventHubName
{
	public EventProcessorClient Client { get; }
	private readonly BlobContainerClient _poisonBlobContainer;

	public EventHubProcessor(IOptions<EventHubOptions> options, THub hubConfig, BlobServiceClient blobServiceClient)
	{
		Client = new EventProcessorClient(
			new BlobContainerClient(options.Value.BlobStorageConnection, hubConfig.CheckpointStoreName),
			EventHubConsumerClient.DefaultConsumerGroupName,
			options.Value.HubConnection,
			hubConfig.HubName
		);

		_poisonBlobContainer = blobServiceClient.GetBlobContainerClient(hubConfig.PoisonMessagesBlobContainerName);
	}

	public async Task MoveToPoisonQueue(EventData eventData, Exception ex)
	{
		var poisonMessage = new PoisonMessage
		{
			MessageId = eventData.MessageId,
			Body = eventData.EventBody.ToArray(),
			ErrorDetails = ex.ToString(),
			OccurredAt = DateTimeOffset.UtcNow,
			Metadata = eventData.Properties as Dictionary<string, object>
		};

		var blobName = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
		var blobClient = _poisonBlobContainer.GetBlobClient(blobName);

		await blobClient.UploadAsync(
			BinaryData.FromObjectAsJson(poisonMessage),
			overwrite: false
		);
	}
}