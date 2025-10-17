using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Vouchers.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class DeleteVoucherCommand : IRequest<bool>
{
    public Guid VoucherId { get; set; }
}

public class DeleteVoucherCommandValidator : AbstractValidator<DeleteVoucherCommand>
{
    public DeleteVoucherCommandValidator()
    {
        RuleFor(x => x.VoucherId)
            .NotEmpty()
            .WithMessage("VoucherId is required");
    }
}

public class DeleteVoucherCommandHandler : IRequestHandler<DeleteVoucherCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Id == request.VoucherId, cancellationToken);

        if (voucher == null)
            throw new ErrorCodeException(ErrorCodes.COMMON_NOT_FOUND, "Voucher not found");

        // Remove product associations first
        var productVouchers = await _context.ProductVouchers
            .Where(pv => pv.VoucherId == request.VoucherId)
            .ToListAsync(cancellationToken);
        
        _context.ProductVouchers.RemoveRange(productVouchers);

        // Remove voucher
        _context.Vouchers.Remove(voucher);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
