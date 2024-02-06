using FluentValidation.Results;
using MediatR;

namespace IoCloud.Shared.MessageHandling.Abstractions
{
    public interface IBaseCommand : IBaseRequest
    {
        ValidationResult ValidationResult { get; set; }
    }
}
