namespace AvvaMobile.EventHubs.Abstractions;

public interface IEventHubProducer<THub>
    where THub : IEventHubName
{
    Task SendAsync<TEvent>(TEvent @event);
}