using IoCloud.Shared.Entity.Abstractions;

namespace IoCloud.Shared.Entity.Extensions
{
    public static class AuditableExtensions
    {
        public static void Stamp(this IAuditable target, string requestedBy)
        {
            if (target != null)
            {
                var now = DateTimeOffset.Now;
                if (target.CreatedOn == default)
                {
                    target.CreatedOn = now;
                }
                if (target.CreatedBy == null)
                {
                    target.CreatedBy = requestedBy;
                }
                target.UpdatedOn = now;
                target.UpdatedBy = requestedBy;
            }
        }
    }
}
