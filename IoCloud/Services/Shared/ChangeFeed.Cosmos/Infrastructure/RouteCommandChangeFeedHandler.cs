using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using IoCloud.Shared.ChangeFeed.Abstractions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Utility;
using System.Dynamic;

namespace IoCloud.Shared.ChangeFeed.Infrastructure
{
    /// <summary>
    /// A Command Handler that performs some action on a dispatched ChangeFeedCommand that contains an entity detected by the Cosmos Change Feed.
    /// Handles heterogenous types of objects, hence the TEntity is of type ExpandoObject.
    /// </summary>
    public class RouteCommandChangeFeedHandler<TRootEntity> : IChangeFeedHandler<ExpandoObject, TRootEntity>
    {
        private readonly IIocMediator _mediator;
        private readonly IServiceProvider _serviceProvider;
        private readonly IIocMediatorFactory _mediatorFactory;
        private readonly ILogger<RouteCommandChangeFeedHandler<TRootEntity>> _logger;

        public RouteCommandChangeFeedHandler
        (
            IServiceProvider serviceProvider,
            IIocMediatorFactory mediatorFactory,
            ILogger<RouteCommandChangeFeedHandler<TRootEntity>> logger
        )
        {
            _serviceProvider = serviceProvider;
            _mediatorFactory = mediatorFactory;
            _logger = logger;
        }

        public async Task HandleChangeAsync(ExpandoObject changed, CancellationToken cancellationToken)
        {
            try
            {
                var dynamicChangedEntity = changed as dynamic;

                Type changedEntityType = TypeUtils.GetType(dynamicChangedEntity.entityType);
                if (changedEntityType == null)
                {
                    throw new Exception($"Unknown entity type {dynamicChangedEntity.entityType}");
                }

                var changedEntityJson = JsonConvert.SerializeObject(dynamicChangedEntity);
                var changedEntity = JsonConvert.DeserializeObject(changedEntityJson, changedEntityType);

                Type changeFeedCommandType = typeof(ChangeFeedCommand<,>);
                Type genericChangeFeedCommandType = changeFeedCommandType.MakeGenericType(changedEntityType, typeof(TRootEntity));
                object changeFeedCommand = Activator.CreateInstance(genericChangeFeedCommandType, changedEntity);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var mediator = _mediatorFactory.CreateMediator
                    (
                        type => scope.ServiceProvider.GetRequiredService(type)
                    );

                    await mediator.Send(changeFeedCommand);
                }
            }
            catch (Exception ex)
                {
                _logger.LogError($"{ex.Message}: Message: {JsonConvert.SerializeObject(changed)}");
                throw;
            }
        }
    }
}
