using AutoMapper;
using IoCloud.Shared.Messages;

namespace IoCloud.Shared.Mapping.Infrastructure
{
    /// <summary>
    /// Derived from Profile with built-in mappings for Cloud Event to Cloud Event Header and serialization
    /// </summary>
    public abstract class CloudEventMappingProfile : Profile
    {
        public CloudEventMappingProfile()
        {
            CreateMap<CloudEventMessage, CloudEventMessageHeader>().ReverseMap();
        }
    }
}
