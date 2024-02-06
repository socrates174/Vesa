using FluentValidation.Results;
using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IQuery<TReply> : IRequest<TReply>
    {
        ValidationResult ValidationResult { get; set; }
    }
}
