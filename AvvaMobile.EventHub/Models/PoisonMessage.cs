namespace AvvaMobile.EventHubs.Models;

public class PoisonMessage
{
    public string MessageId { get; set; }
    public byte[] Body { get; set; }
    public string ErrorDetails { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}