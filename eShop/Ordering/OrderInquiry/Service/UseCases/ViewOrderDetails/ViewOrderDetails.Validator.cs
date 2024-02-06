using FluentValidation;

namespace eShop.Ordering.OrderInquiry.Service.UseCases.ViewOrderDetails
{
    public class ViewOrderDetailsValidator : AbstractValidator<ViewOrderDetailsQuery>
    {
        public ViewOrderDetailsValidator()
        {
            RuleFor(query => query.Id).NotNull();
        }
    }
}
