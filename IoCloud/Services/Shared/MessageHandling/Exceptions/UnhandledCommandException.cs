namespace IoCloud.Shared.MessageHandling.Exceptions
{
    public class UnhandledCommandException : Exception
    {
        public UnhandledCommandException() : base()
        {
        }

        public UnhandledCommandException(string? message) : base(message)
        {
        }
        public UnhandledCommandException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}