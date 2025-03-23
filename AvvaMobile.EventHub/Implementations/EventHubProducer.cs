using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using AvvaMobile.EventHubs.Abstractions;
using Azure.Messaging.EventHubs;
using AvvaMobile.EventHubs.Configurations;

namespace AvvaMobile.EventHubs.Implementations;

public class EventHubProducer<THub> : IEventHubProducer<THub>, IDisposable
    where THub : IEventHubName
{
    private readonly EventHubProducerClient _client;
    
    public EventHubProducer(IOptions<EventHubOptions> settings, THub hubConfig)
    {
        _client = new EventHubProducerClient(settings.Value.HubConnection, hubConfig.HubName);
    }
    
    public async Task SendAsync<TEvent>(TEvent @event)
    {
        var message = JsonSerializer.Serialize(@event);
        
        using var eventBatch = await _client.CreateBatchAsync();
        var eventData = new EventData(Encoding.UTF8.GetBytes(message));
        if (!eventBatch.TryAdd(eventData))
        {
            throw new Exception($"Mesaj eklenemedi: {message}");
        }
        
        await _client.SendAsync(eventBatch);
    }
    
    public void Dispose()
    {
        _client?.DisposeAsync().AsTask().Wait();
    }
}