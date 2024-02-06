using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;
using IoCloud.Shared.Utility;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders
{
    public class ViewOrdersHandler : IQueryHandler<ViewOrdersQuery, IEnumerable<ViewOrdersReply>>, IDisposable
    {
        private readonly INoSqlQueryRepository<Order> _repository;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public ViewOrdersHandler(INoSqlQueryRepository<Order> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ViewOrdersReply>> Handle(ViewOrdersQuery query, CancellationToken cancellationToken)
        {

            return await _repository.GetQuery().Where
            (
                o => o.EmailAddress == query.EmailAddress
            )
            .Select(o => new ViewOrdersReply(o.Id, o.DateOrdered, o.PaymentAmount))
            .ToListAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _repository.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ViewOrdersHandler()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}