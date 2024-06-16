using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TableDependency.SqlClient.Base.Abstracts;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;
using vesa.Core.Abstractions;
using vesa.Core.Helpers;
using vesa.Core.Infrastructure;
using ErrorEventArgs = TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs;

namespace vesa.SQL.Infrastructure;

public class SQLEventStoreListener : IEventStoreListener
{
    private CancellationToken _cancellationToken;
    private ITableDependency<EventJson> _eventJsonChangeFeed;
    private readonly IEventProcessor _eventProcessor;
    private readonly ILogger<SQLEventStoreListener> _logger;

    public SQLEventStoreListener
    (
        ITableDependency<EventJson> eventJsonChangeFeed,
        IEventProcessor eventProcessor,
        ILogger<SQLEventStoreListener> logger
    )
    {
        _eventJsonChangeFeed = eventJsonChangeFeed;
        _eventProcessor = eventProcessor;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _eventJsonChangeFeed.OnChanged += Changed;
        _eventJsonChangeFeed.Start();
    }

    public async Task StopAsync()
    {
        _eventJsonChangeFeed.Stop();
    }

    private void Changed(object sender, RecordChangedEventArgs<EventJson> e)
    {
        if (_cancellationToken.IsCancellationRequested)
        {
            StopAsync().ConfigureAwait(false);
        }
        if (e.ChangeType == ChangeType.Insert)
        {
            var newEventJson = e.Entity;
            var newEvent = (IEvent)JsonConvert.DeserializeObject(newEventJson.Json, TypeHelper.GetType(newEventJson.EventTypeName));
            _eventProcessor.ProcessAsync(newEvent, _cancellationToken).GetAwaiter().GetResult();
        }
    }

    private static void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine(e.Message);
        Console.WriteLine(e.Error?.InnerException?.Message);
    }
}