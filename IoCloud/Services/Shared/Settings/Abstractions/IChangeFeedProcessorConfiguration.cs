namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IChangeFeedProcessorConfiguration
    {
        string SourceDatabaseName { get; set; }
        string SourceContainerName { get; set; }
        string LeaseDatabaseName { get; set; }
        string LeaseContainerName { get; set; }
        string ProcessorName { get; set; }

        // Example: "2022/08/20 10:45:00 PM -05:00"
        DateTimeOffset? StartDateTimeOffset { get; set; }
    }
}