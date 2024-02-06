using Newtonsoft.Json;

namespace IoCloud.Shared.Utility
{
    public static class ObjectExtensions
    {
        public static T Clone<T>(this T instance)
        {
            var json = JsonConvert.SerializeObject(instance);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
