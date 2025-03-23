using AvvaMobile.EventHubs.Abstractions;
using AvvaMobile.EventHubs.Configurations;
using AvvaMobile.EventHubs.Implementations;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Mlink.AdDistribution.Shared.EventHubs.DependencyInjection;

public static class Registrations
{
	public static IServiceCollection AddEventHubProducer<THub>(this IServiceCollection services, IConfiguration configuration)
		where THub : class, IEventHubName
	{
		services.AddSingleton<IConfigureOptions<EventHubOptions>>(
		new ConfigureFromConfigurationOptions<EventHubOptions>(
			configuration.GetSection("EventHubOptions")));

		services.AddSingleton<THub>();
		services.AddSingleton<IEventHubProducer<THub>, EventHubProducer<THub>>();
		return services;
	}

	public static IServiceCollection AddEventHubConsumer<TConsumer, THub>(this IServiceCollection services, IConfiguration configuration)
		where TConsumer : BackgroundService
		where THub : class, IEventHubName
	{
		services.Configure<EventHubOptions>(configuration.GetSection("EventHubOptions").Bind);

		var eventHubOptions = configuration.GetSection("EventHubOptions").Get<EventHubOptions>();

		services.AddSingleton(provider => new BlobServiceClient(eventHubOptions.BlobStorageConnection));

		services.AddSingleton<THub>();

		services.AddSingleton<IEventHubProcessor<THub>, EventHubProcessor<THub>>();

		services.AddHostedService<TConsumer>();
		return services;
	}
}
