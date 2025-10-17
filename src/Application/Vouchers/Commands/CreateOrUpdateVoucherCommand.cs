using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Vouchers.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class CreateOrUpdateVoucherCommand : IRequest<Guid>
{
    public Guid? VoucherId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty; // PERCENT or FIXED_AMOUNT
    public long DiscountValue { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<Guid> ProductIds { get; set; } = new();
}

public class CreateOrUpdateVoucherCommandValidator : AbstractValidator<CreateOrUpdateVoucherCommand>
{
    private static readonly string[] AllowedDiscountTypes = { "PERCENT", "FIXED_AMOUNT" };

    public CreateOrUpdateVoucherCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Code is required and must not exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.DiscountType)
            .NotEmpty()
            .Must(x => AllowedDiscountTypes.Contains(x))
            .WithMessage("DiscountType must be either PERCENT or FIXED_AMOUNT");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0)
            .WithMessage("DiscountValue must be greater than 0");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == "PERCENT")
            .WithMessage("Discount percentage must not exceed 100%");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("StartDate is required");

        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("EndDate is required and must be after StartDate");
    }
}

public class CreateOrUpdateVoucherCommandHandler : IRequestHandler<CreateOrUpdateVoucherCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrUpdateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrUpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        // Validate that code is unique (except for current voucher if updating)
        var existingVoucherWithCode = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == request.Code && v.Id != request.VoucherId, cancellationToken);

        if (existingVoucherWithCode != null)
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_REQUEST, "Voucher code already exists");

        // Validate that all products exist
        if (request.ProductIds.Any())
        {
            var existingProductsCount = await _context.Products
                .CountAsync(p => request.ProductIds.Contains(p.Id) && !p.IsDeleted, cancellationToken);

            if (existingProductsCount != request.ProductIds.Count)
                throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "One or more products not found");
        }

        Voucher? voucher;

        if (request.VoucherId.HasValue)
        {
            // Update existing voucher
            voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Id == request.VoucherId.Value, cancellationToken);

            if (voucher == null)
                throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Voucher not found");

            // voucher.Code = request.Code;
            voucher.Description = request.Description;
            voucher.DiscountType = request.DiscountType;
            voucher.DiscountValue = request.DiscountValue;
            voucher.StartDate = request.StartDate;
            voucher.EndDate = request.EndDate;
            voucher.IsActive = request.IsActive;

            // Remove existing product associations
            var existingProductVouchers = await _context.ProductVouchers
                .Where(pv => pv.VoucherId == voucher.Id)
                .ToListAsync(cancellationToken);
            _context.ProductVouchers.RemoveRange(existingProductVouchers);
        }
        else
        {
            // Create new voucher
            voucher = new Voucher
            {
                Code = request.Code,
                Description = request.Description,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsActive = request.IsActive,
                UsedCount = 0
            };

            _context.Vouchers.Add(voucher);
        }

        // Add product associations
        foreach (var productId in request.ProductIds)
        {
            var productVoucher = new ProductVoucher
            {
                ProductId = productId,
                VoucherId = voucher.Id
            };
            _context.ProductVouchers.Add(productVoucher);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return voucher.Id;
    }
}
