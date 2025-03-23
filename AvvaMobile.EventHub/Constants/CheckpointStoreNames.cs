namespace AvvaMobile.EventHubs.Constants;

public static class CheckpointStoreNames
{
    public static string ExampleEventCheckpointStore => GetHubName("example-event-checkpoint-store");

    private static string GetHubName(string eventName)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        return environment switch
        {
            "Development" => $"dev-{eventName}",
            "Staging" => $"staging-{eventName}",
            "Production" => eventName,
            _ => $"dev-{eventName}"
        };
    }
}