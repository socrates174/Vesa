using AutoMapper;
using Entities = eShop.Ordering.OrderInquiry.Data.Entities;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrderDetails
{
    public class ViewOrderDetailsMapping : Profile
    {
        public ViewOrderDetailsMapping()
        {
            CreateMap<Entities.Order, ViewOrderDetailsReply>();
        }
    }
}
