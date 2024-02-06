using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using IoCloud.Shared.Settings.Abstractions;
using IoCloud.Shared.Settings.Infrastructure;
using IoCloud.Shared.Storage.Blobs.Infrastructure;
using IoCloud.Shared.Storage.Documents.Abstractions;

namespace IoCloud.Shared.Storage.Documents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BlobStorageConfiguration>(configuration.GetSection(nameof(BlobStorageConfiguration)));
            var blobStorageConfiguration = services.BuildServiceProvider().GetRequiredService<IOptions<BlobStorageConfiguration>>().Value;
            services.AddSingleton<IBlobStorageConfiguration>(blobStorageConfiguration);

            var blobClientOptions = new BlobClientOptions();
            blobClientOptions.Retry.MaxRetries = blobStorageConfiguration.MaxRetries;
            blobClientOptions.Retry.Delay = TimeSpan.FromSeconds(blobStorageConfiguration.DelayInSeconds);
            blobClientOptions.Diagnostics.IsLoggingEnabled = blobStorageConfiguration.IsLoggingEnabled;
            blobClientOptions.Diagnostics.ApplicationId = blobStorageConfiguration.ApplicationId;
            //blobClientOptions.AddPolicy(provider.GetService<TracingPolicy>(), HttpPipelinePosition.PerCall);

            var blobServiceClient = new BlobServiceClient(blobStorageConfiguration.ConnectionStringKey, blobClientOptions);
            services.AddSingleton(blobServiceClient);

            services.AddScoped<IDocumentStorage, BlobStorage>();

            return services;
        }
 
        public static IServiceCollection AddBlobContainerClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BlobStorageConfiguration>(configuration.GetSection(nameof(BlobStorageConfiguration)));
            var blobStorageConfiguration = services.BuildServiceProvider().GetRequiredService<IOptions<BlobStorageConfiguration>>().Value;
            services.AddSingleton<IBlobStorageConfiguration>(blobStorageConfiguration);

            var blobContainerClient = new BlobContainerClient(blobStorageConfiguration.ConnectionStringKey, blobStorageConfiguration.ContainerName);
            services.AddSingleton(blobContainerClient);

            return services;
        }
    }
}