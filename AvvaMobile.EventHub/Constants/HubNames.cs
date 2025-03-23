namespace AvvaMobile.EventHubs.Constants;

public static class HubNames
{
    public static string ExampleEvent => GetHubName("example-event");
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