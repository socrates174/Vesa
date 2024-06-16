using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Abstracts;
using vesa.Core.Abstractions;
using vesa.Core.Infrastructure;
using vesa.SQL.Infrastructure;

namespace vesa.SQL.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSQLStore<TDbContext>
    (
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifeTime = ServiceLifetime.Scoped
    )
        where TDbContext : DbContext
    {
        services.AddDbContext<DbContext, TDbContext>(opts => opts.UseSqlServer(configuration[configuration["SqlConnectionKey"]]), serviceLifeTime);
        //using (var scope = services.BuildServiceProvider().CreateScope())
        //{
        //var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
        //dbContext.Database.EnsureCreated();
        //}
        return services;
    }

    public static IServiceCollection AddSQLEventListeners
    (
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
    )
    {
        services.Add(new ServiceDescriptor(typeof(IEventListener), typeof(SQLEventStoreListener), serviceLifetime));
        services.Add(new ServiceDescriptor
        (
            typeof(ITableDependency<EventJson>),
            sp => new SqlTableDependency<EventJson>(configuration[configuration["SqlConnectionKey"]]),
            serviceLifetime)
        );
        services.Add(new ServiceDescriptor(typeof(IEventProcessor), typeof(EventProcessor), serviceLifetime));

        return services;
    }
}