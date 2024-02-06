namespace IoCloud.Shared.MessageHandling.Validation
{
    [Serializable]
    public class ValidationErrorMessage
    {
        public ValidationErrorMessage(string message, string? type = null, string? path = null)
        {
            Message = message;
            Type = type;
            Path = path;
        }

        public string Message { get; init; }

        public string? Path { get; init; }

        public string? Type { get; init; }
    }
}
