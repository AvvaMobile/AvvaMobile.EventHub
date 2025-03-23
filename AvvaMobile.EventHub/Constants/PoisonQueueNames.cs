namespace AvvaMobile.EventHubs.Constants;

public static class PoisonQueueNames
{
    public static string ExampleEventPoisonQueue => GetHubName("example-event-poison-queue");

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