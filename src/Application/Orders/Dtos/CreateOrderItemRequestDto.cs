using CleanArchitectureBase.Application.Common.Extensions;

namespace CleanArchitectureBase.Application.Orders.Dtos;

public class CreateOrderItemRequestDto
{
    public Guid? ProductVariantId { get; set; }
    public Guid? DesignId { get; set; }
    public int Quantity { get; set; }
    // public string? VoucherCode { get; set; }
}

public class CreateOrderRequestDtoValidator : AbstractValidator<CreateOrderItemRequestDto>
{
    public CreateOrderRequestDtoValidator()
    {
        RuleFor(x => x.ProductVariantId).NotEmpty();

        RuleFor(x => x.DesignId)
            .NullOrNotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThan(0);
        // RuleFor(x => x.VoucherCode).NullOrNotEmpty();
    }
}



