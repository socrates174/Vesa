using eShop.Ordering.OrderManagement.Data.Entities;
using eShop.Ordering.OrderManagement.EntityMovement.Extensions;
using IoCloud.Shared.ChangeFeed.Infrastructure;
using System.Dynamic;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure(hostContext.Configuration);
        services.AddHostedService<ChangeFeedProcessingService<ExpandoObject, Order>>();
    })
    .Build();

await host.RunAsync();
