using eShop.Ordering.OrderInquiry.Data.Entities;
using eShop.Ordering.OrderInquiry.HubAPI.Extensions;
using IoCloud.Shared.Messaging.Consumption.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure(hostContext.Configuration);
        services.AddHostedService<EventHubCloudEventConsumptionService<InboxEvent>>();
    })
    .Build();

await host.RunAsync();
