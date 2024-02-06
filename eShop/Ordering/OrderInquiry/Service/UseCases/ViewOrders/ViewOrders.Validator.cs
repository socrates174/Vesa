using FluentValidation;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrders
{
    public class ViewOrdersValidator : AbstractValidator<ViewOrdersQuery>
    {
        public ViewOrdersValidator()
        {
            RuleFor(query => query.EmailAddress).NotNull();
        }
    }
}
