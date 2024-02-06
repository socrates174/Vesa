using AutoMapper;
using eShop.Ordering.OrderInquiry.Data.Entities;
using IoCloud.Shared.Mapping.Infrastructure;
using IoCloud.Shared.Messages;
using IoCloud.Shared.Messages.Extensions;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.OrderPlaced
{
    public class OrderPlacedMapping : CloudEventMappingProfile
    {
        public OrderPlacedMapping()
        {
            CreateMap<OrderPlacedEvent, Order>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.OrderId));

            CreateMap<OrderPlacedEvent, InboxEvent>()
                .IncludeMembers(src => src.Header)  // flattens header properties to root properties
                .ForMember(dest => dest.Id, opt => opt.Ignore())    // event generates its own Id so do not overwrite if Id in the payload    
                .ForMember(dest => dest.Data, opt => opt.MapFrom(src => src.ToExpandoObjectWithoutHeader()));   // Assign json without the header into Data property

            CreateMap<CloudEventMessageHeader, InboxEvent>(MemberList.Source); // prevents exception if target has properties not in header
        }
    }
}
