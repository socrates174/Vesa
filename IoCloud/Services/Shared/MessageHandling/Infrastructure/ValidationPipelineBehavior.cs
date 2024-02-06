using FluentValidation;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Exceptions;
using IoCloud.Shared.MessageHandling.Validation;
using MediatR;

namespace IoCloud.Shared.MessageHandling.Infrastructure
{
    /// <summary>
    /// Validate a command/event/query as part of a pipeline between when the command/event/query is received and is handled
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="TReply"></typeparam>
    public class ValidationPipelineBehavior<TCommand, TReply> : IPipelineBehavior<TCommand, TReply>
        where TCommand : ICommand<TReply>
    {
        private readonly IEnumerable<IValidator<TCommand>> _validators;
        public ValidationPipelineBehavior(IEnumerable<IValidator<TCommand>> validators) => _validators = validators;

        public async Task<TReply> Handle(TCommand command, RequestHandlerDelegate<TReply> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await next();
            }

            var errors = _validators
                .Select(v => v.Validate(command))
                .SelectMany(x => x.Errors)
                .Where(x => x != null);

            if (errors.Any())
            {
                throw new ApplicationValidationException(errors.Select(e => new ValidationErrorMessage(e.ErrorMessage, e.ErrorCode, e.PropertyName)));
            }

            return await next();
        }
    }
}
