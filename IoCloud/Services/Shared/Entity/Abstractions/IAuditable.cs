namespace IoCloud.Shared.Entity.Abstractions
{
    public interface IAuditable
    {
        DateTimeOffset CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTimeOffset UpdatedOn { get; set; }
        string UpdatedBy { get; set; }
    }
}
