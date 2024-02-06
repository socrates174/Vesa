using FluentValidation;

namespace eShop.Ordering.OrderManagement.Service.UseCases.PlaceOrder
{
    public class CancelOrderValidator : AbstractValidator<PlaceOrderCommand>
    {
        public CancelOrderValidator()
        {
            RuleFor(command => command.DateOrdered).NotNull();
            RuleFor(command => command.Items.Count).GreaterThan(0);
            RuleFor(command => command.Buyer).NotNull();
            RuleFor(command => command.ShippingAddress).NotNull();
            RuleFor(command => command.Payment).NotNull();
        }
    }
}
