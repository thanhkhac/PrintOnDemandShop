using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitectureBase.Application.TokenPackages.Queries;

public class CheckTokenPackageIsPaidRequest : IRequest<bool>
{
    public string? PaymentCode { get; set; }
}

public class CheckTokenPackageIsPaidHandler : IRequestHandler<CheckTokenPackageIsPaidRequest, bool>
{
    private readonly IApplicationDbContext _context;

    public CheckTokenPackageIsPaidHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(CheckTokenPackageIsPaidRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentCode))
            throw new ErrorCodeException(ErrorCodes.COMMON_INVALID_REQUEST, "PaymentCode is required");

        var tokenPackageOrder = await _context.UserTokenPackages
            .FirstOrDefaultAsync(x => x.PaymentCode == request.PaymentCode, cancellationToken);

        if (tokenPackageOrder == null)
            throw new ErrorCodeException(ErrorCodes.TOKEN_PACKAGE_NOT_FOUND, "Không tìm thấy đơn token package tương ứng");

        return tokenPackageOrder.IsPaid;
    }
}
