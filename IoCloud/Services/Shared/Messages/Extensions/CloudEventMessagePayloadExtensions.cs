using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;

namespace IoCloud.Shared.Messages.Extensions
{
    public static class CloudEventMessagePayloadExtensions
    {
        public static ExpandoObject ToExpandoObjectWithoutHeader(this ICloudEventMessagePayload messagePayload)
            => JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(messagePayload), new ExpandoObjectConverter());

        public static ExpandoObject ToExpandoObjectWithoutHeader(this CloudEventMessagePayload messagePayload)
            => JsonConvert.DeserializeObject<ExpandoObject>(JsonConvert.SerializeObject(messagePayload), new ExpandoObjectConverter());
    }
}
