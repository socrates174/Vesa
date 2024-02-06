namespace IoCloud.Shared.Entity.Abstractions
{
    public interface IOptimisticConcurrency
    {
        string ConcurrencyToken { get; set; }
    }
}
