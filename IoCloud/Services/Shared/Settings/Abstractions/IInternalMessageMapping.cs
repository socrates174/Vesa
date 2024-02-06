namespace IoCloud.Shared.Settings.Abstractions
{
    public interface IInternalMessageMapping : IMessageMapping
    {
        string InternalType { get; set; }
    }
}