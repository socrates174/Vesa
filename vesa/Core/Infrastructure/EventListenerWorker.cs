using vesa.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace vesa.Core.Infrastructure;

public class EventListenerWorker : BackgroundService
{
    private IEnumerable<IEventListener> _eventListeners;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventListenerWorker> _logger;

    public EventListenerWorker(IServiceProvider serviceProvider, ILogger<EventListenerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var tasks = new List<Task>();
                _eventListeners = scope.ServiceProvider.GetRequiredService<IEnumerable<IEventListener>>();
                foreach (var listener in _eventListeners)
                {
                    tasks.Add(listener.StartAsync(stoppingToken));
                }
                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {

        }

        //return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");
        foreach (var listener in _eventListeners)
        {
            listener.StopAsync();
        }
        await base.StopAsync(stoppingToken);
    }
}