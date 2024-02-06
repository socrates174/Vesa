namespace IoCloud.Shared.Entity.Abstractions
{
    public interface ISoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}