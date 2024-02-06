using System.Text.Json;

namespace IoCloud.Shared.Utility
{
    public class PascalCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToTitleCase().Replace("_", string.Empty);
    }
}
