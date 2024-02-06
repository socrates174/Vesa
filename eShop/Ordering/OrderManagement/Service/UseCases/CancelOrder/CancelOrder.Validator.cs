using FluentValidation;

namespace eShop.Ordering.OrderManagement.Service.UseCases.CancelOrder
{
    public class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
    {
        public CancelOrderValidator()
        {
            RuleFor(command => command.OrderId).NotNull();
        }
    }
}
