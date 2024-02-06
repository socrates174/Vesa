using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.File.Abstractions;
using vesa.File.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace vesa.File.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSequenceNumberGenerator(this IServiceCollection services, IConfiguration configuration)
    {
        var sequenceNumberPath = configuration["SequenceNumberPath"];
        if (!@Directory.Exists(sequenceNumberPath))
        {
            Directory.CreateDirectory(sequenceNumberPath);
        }
        services.AddSingleton<ISequenceNumberGenerator>(_ => new FileSequenceNumberGenerator(configuration["SequenceNumberPath"]));

        return services;
    }

    public static IServiceCollection AddFileEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        var eventStorePath = configuration["EventStorePath"];

        if (!@Directory.Exists(eventStorePath))
        {
            Directory.CreateDirectory(eventStorePath);
        }

        services.AddSingleton<IEventStore>(sp => new FileEventStore
        (
            configuration["EventStorePath"],
            sp.GetService<ILogger<FileEventStore>>()
        ));

        services.AddSingleton<IFileEventStore>(sp => new FileEventStore
        (
            configuration["EventStorePath"],
            sp.GetService<ILogger<FileEventStore>>()
        ));

        return services;
    }

    public static IServiceCollection AddFileEventListeners(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventListener, FileEventStoreListener>();
        services.AddSingleton<FileEventStoreListener>();
        //services.AddSingleton<IEventListener, FileEventHubListener>();
        //services.AddSingleton<FileEventHubListener>();
        services.AddSingleton<IEventProcessor, EventProcessor>();
        return services;
    }

    public static IServiceCollection AddFileEventHub(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddSingleton<IEventConsumer>(_ => new FileEventConsumer(configuration["EventHubPath"], configuration["EventConsumerId"],));
        //services.AddSingleton<IFileEventConsumer>(_ => new FileEventConsumer(configuration["EventHubPath"], configuration["EventConsumerId"]));

        // Event Publisher
        services.AddSingleton<IEventPublisher>(_ => new FileEventPublisher(configuration["EventHubPath"]));
        return services;
    }
}
