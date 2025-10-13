using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Orders.Admin.Queries;

public class GetAllOrdersQueryValidator : PaginatedQueryValidator<GetAllOrdersQuery>
{
    public GetAllOrdersQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || 
                           new[] { "Pending", "Approved", "Rejected", "Cancelled", "Completed" }.Contains(status))
            .WithMessage("Status must be one of: Pending, Approved, Rejected, Cancelled, Completed");

        RuleFor(x => x.CustomerName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.CustomerName));

        RuleFor(x => x.CustomerEmail)
            .MaximumLength(255)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.CustomerEmail));
    }
}
