namespace CleanArchitectureBase.Application.Common.Models;

public class PaginatedQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public abstract class PaginatedQueryValidator<T> : AbstractValidator<T> where T : PaginatedQuery
{
    protected PaginatedQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
