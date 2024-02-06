using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.Querying.NoSql.Abstractions;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrderDetails
{
    public class ViewOrderDetailsHandler : IQueryHandler<ViewOrderDetailsQuery, ViewOrderDetailsReply>, IDisposable
    {
        private readonly INoSqlQueryRepository<Order> _repository;
        private readonly IMapper _mapper;
        private bool disposedValue;

        public ViewOrderDetailsHandler(INoSqlQueryRepository<Order> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ViewOrderDetailsReply> Handle(ViewOrderDetailsQuery query, CancellationToken cancellationToken)
        {
            var order = await _repository.GetAsync(query.Id);
            return _mapper.Map<Order, ViewOrderDetailsReply>(order);
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
        // ~ViewOrderDetailsHandler()
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
