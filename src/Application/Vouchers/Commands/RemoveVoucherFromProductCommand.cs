using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Vouchers.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class RemoveVoucherFromProductCommand : IRequest<bool>
{
    public Guid VoucherId { get; set; }
    public List<Guid> ProductIds { get; set; } = new();
}

public class RemoveVoucherFromProductCommandValidator : AbstractValidator<RemoveVoucherFromProductCommand>
{
    public RemoveVoucherFromProductCommandValidator()
    {
        RuleFor(x => x.VoucherId)
            .NotEmpty()
            .WithMessage("VoucherId is required");

        RuleFor(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("At least one ProductId is required");

        RuleForEach(x => x.ProductIds)
            .NotEmpty()
            .WithMessage("ProductId cannot be empty");
    }
}

public class RemoveVoucherFromProductCommandHandler : IRequestHandler<RemoveVoucherFromProductCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveVoucherFromProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveVoucherFromProductCommand request, CancellationToken cancellationToken)
    {
        // Find existing associations to remove
        var productVouchersToRemove = await _context.ProductVouchers
            .Where(pv => pv.VoucherId == request.VoucherId && request.ProductIds.Contains(pv.ProductId))
            .ToListAsync(cancellationToken);

        if (!productVouchersToRemove.Any())
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "No voucher-product associations found to remove");

        _context.ProductVouchers.RemoveRange(productVouchersToRemove);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
