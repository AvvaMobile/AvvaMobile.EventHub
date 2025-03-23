namespace AvvaMobile.EventHubs.Abstractions;

public interface IEventHubName
{
    string HubName { get; }
    string CheckpointStoreName { get; }
    string PoisonMessagesBlobContainerName { get; }
}