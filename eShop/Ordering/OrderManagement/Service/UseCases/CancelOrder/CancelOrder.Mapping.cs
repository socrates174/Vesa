using AutoMapper;
using eShop.Ordering.OrderManagement.Data.Entities;
using IoCloud.Shared.Mapping.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messages.Extensions;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class CancelOrderMapping : CloudEventMappingProfile
    {
        public CancelOrderMapping() : base()
        {
            CreateMap<Order, CancelOrderDomain>().ReverseMap();

            CreateMap<CancelOrderCommand, InboxCommand>()
                .IncludeMembers(src => src.Header)  // flattens header properties to root properties
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // event generates its own Id so do not overwrite if Id in the payload    
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.ToExpandoObjectWithoutHeader()));   // Assign json without the header into Data property

            CreateMap<CloudEventMessageHeader, InboxCommand>(MemberList.Source); // prevents exception if target has properties not in header

            CreateMap<OrderCancelledEvent, OutboxEvent>()
                .IncludeMembers(src => src.Header)  // flattens header properties to root properties
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // event generates its own Id so do not overwrite if Id in the payload    
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.ToExpandoObjectWithoutHeader()));   // Assign json without the header into Data property

            CreateMap<CloudEventMessageHeader, OutboxEvent>(MemberList.Source); // prevents exception if target has properties not in header
        }
    }
}
