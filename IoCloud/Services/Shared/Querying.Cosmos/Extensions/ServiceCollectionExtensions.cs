using IoCloud.Shared.Querying.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;
using IoCloud.Shared.Querying.NoSql.Infrastructure;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Settings.Infrastructure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IoCloud.Shared.Querying.Cosmos.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosClientQ
        (
            this IServiceCollection services,
            IConfiguration configuration,
            string cosmosClientConfigurationSection = "CosmosClientConfiguration"
        )
        {
            services.Configure<CosmosClientConfiguration>(cosmosClientConfigurationSection, configuration.GetSection(cosmosClientConfigurationSection));
            var cosmosClientConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<CosmosClientConfiguration>>().Get(cosmosClientConfigurationSection);

            CosmosClientOptions cosmosClientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                },
                MaxRetryAttemptsOnRateLimitedRequests = cosmosClientConfiguration.MaxRetryAttemptsOnRateLimitedRequests,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(cosmosClientConfiguration.MaxRetryWaitTimeOnRateLimitedRequestsInSeconds),
            };

            CosmosClient cosmosClient = new CosmosClient(configuration[cosmosClientConfiguration.UrlKey], configuration[cosmosClientConfiguration.AuthKey], cosmosClientOptions);

            services.AddSingleton(cosmosClient);

            return services;
        }

        public static IServiceCollection AddCosmosContainerConfigurationQ<TRootEntity>
        (
            this IServiceCollection services,
            IConfiguration configuration,
             string cosmosContainerConfigurationSection = "CosmosContainerConfiguration"
       )
            where TRootEntity : class
        {
            services.Configure<CosmosContainerConfiguration<TRootEntity>>(cosmosContainerConfigurationSection, configuration.GetSection(cosmosContainerConfigurationSection));
            var cosmosContainerConfiguration = services.BuildServiceProvider().GetRequiredService<IOptionsSnapshot<CosmosContainerConfiguration<TRootEntity>>>().Get(cosmosContainerConfigurationSection);
            services.AddSingleton<ICosmosContainerConfiguration<TRootEntity>>(cosmosContainerConfiguration);

            return services;
        }

        public static IServiceCollection InitializeDatabaseQ(this IServiceCollection services, IConfiguration configuration)
        {
            var serviceProvider = services.BuildServiceProvider();
            var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            var containerConfigurationTypes = services.Where(s => s.ServiceType.IsGenericType && s.ServiceType.GetGenericTypeDefinition() == typeof(ICosmosContainerConfiguration<>)).Select(s => s.ServiceType);
            foreach (var containerConfigurationType in containerConfigurationTypes)
            {
                var containerConfiguration = serviceProvider.GetRequiredService(containerConfigurationType) as CosmosContainerConfiguration;
                InitializeContainerQ(cosmosClient, containerConfiguration);
            }

            return services;
        }

        private static void InitializeContainerQ(CosmosClient cosmosClient, CosmosContainerConfiguration cosmosContainerConfiguration, int throughput = 400)
        {
            var databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync
            (
                cosmosContainerConfiguration.DatabaseName,
                throughput
            )
            .GetAwaiter()
            .GetResult();

            if (cosmosContainerConfiguration.UniqueKeyPaths != null && cosmosContainerConfiguration.UniqueKeyPaths.Any())
            {
                var uniqueKeyPolicy = new UniqueKeyPolicy();
                foreach (var uniqueKeyPath in cosmosContainerConfiguration.UniqueKeyPaths)
                {
                    var uniqueKey = new UniqueKey();
                    foreach (var property in uniqueKeyPath.Split(','))
                    {
                        uniqueKey.Paths.Add(property);

                    };
                    uniqueKeyPolicy.UniqueKeys.Add(uniqueKey);
                }

                ContainerProperties containerProperties = new ContainerProperties()
                {
                    Id = cosmosContainerConfiguration.ContainerName,
                    PartitionKeyPath = cosmosContainerConfiguration.PartitionKeyPath,
                    UniqueKeyPolicy = uniqueKeyPolicy
                };

                ContainerResponse response = databaseResponse.Database.CreateContainerIfNotExistsAsync
                (
                    containerProperties,
                    400
                )
                .GetAwaiter()
                .GetResult();
            }
            else
            {
                var cosmosContainerResponse = databaseResponse.Database.CreateContainerIfNotExistsAsync
                (
                    cosmosContainerConfiguration.ContainerName,
                    cosmosContainerConfiguration.PartitionKeyPath,
                    400
                )
                .GetAwaiter()
                .GetResult();
            }
        }

        public static IServiceCollection AddCosmosQuerying(this IServiceCollection services)
        {

            services.AddScoped(typeof(INoSqlQueryRepository<>), typeof(CosmosQueryRepository<>));
            services.AddScoped(typeof(INoSqlQueryRepository<,>), typeof(CosmosQueryRepository<,>));
            services.AddSingleton(typeof(IQueryExistence<,>), typeof(CosmosQueryRepository<,>));

            return services;
        }
    }
}

