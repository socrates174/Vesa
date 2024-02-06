namespace IoCloud.Shared.MessageHandling.Exceptions
{
    public class UnhandledEventException : Exception
    {
        public UnhandledEventException() : base()
        {
        }

        public UnhandledEventException(string? message) : base(message)
        {
        }
        public UnhandledEventException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}