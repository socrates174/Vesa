using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.EventPublication.Extensions;
using IoCloud.Shared.ChangeFeed.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure(hostContext.Configuration);
        services.AddHostedService<ChangeFeedProcessingService<OutboxEvent>>();
    })
    .Build();

await host.RunAsync();