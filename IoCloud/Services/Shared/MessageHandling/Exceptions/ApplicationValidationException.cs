using System.Runtime.Serialization;
using IoCloud.Shared.MessageHandling.Validation;

namespace IoCloud.Shared.MessageHandling.Exceptions
{
    [Serializable]
    public class ApplicationValidationException : Exception
    {
        private readonly List<ValidationErrorMessage> messages = new();

        public ApplicationValidationException(string errorMessage)
            : this(new ValidationErrorMessage(errorMessage)) { }

        public ApplicationValidationException(ValidationErrorMessage message)
            : this(new[] { message }) { }

        public ApplicationValidationException(IEnumerable<ValidationErrorMessage> messages) : base("One or more validation errors occurred.")
        {
            this.messages = (messages ?? Enumerable.Empty<ValidationErrorMessage>()).ToList();
        }

        public IReadOnlyList<ValidationErrorMessage> Messages { get => this.messages.AsReadOnly(); }

        protected ApplicationValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            messages = (List<ValidationErrorMessage>?)info?.GetValue(nameof(messages), typeof(List<ValidationErrorMessage>))
                ?? new();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info?.AddValue(nameof(messages), messages);
        }
    }
}
