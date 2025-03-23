namespace AvvaMobile.EventHubs.Constants;

public static class SubscriptionNames
{
    public static string EventConsumer => GetSubscriptionName("example-event-consumer");

    private static string GetSubscriptionName(string subscriptionName)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        return environment switch
        {
            "Development" => $"dev-{subscriptionName}",
            "Staging" => $"staging-{subscriptionName}",
            "Production" => subscriptionName,
            _ => $"dev-{subscriptionName}"
        };
    }
}