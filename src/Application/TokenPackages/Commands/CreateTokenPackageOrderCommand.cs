using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TokenPackages.Commands;

public class TokenPackageOrderDto
{
    public string? PaymentCode { get; set; }
    public long Amount { get; set; }
}

[Authorize]
public class CreateTokenPackageOrderCommand : IRequest<TokenPackageOrderDto>
{
    public Guid TokenPackageId { get; set; }
}

public class CreateTokenPackageOrderCommandHandler : IRequestHandler<CreateTokenPackageOrderCommand, TokenPackageOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public CreateTokenPackageOrderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<TokenPackageOrderDto> Handle(CreateTokenPackageOrderCommand request, CancellationToken cancellationToken)
    {
        var tokenPackage = await _context.TokenPackages.FindAsync(request.TokenPackageId);
        if (tokenPackage == null)
            throw new ErrorCodeException(ErrorCodes.TOKEN_PACKAGE_NOT_FOUND);

        var order = new UserTokenPackage
        {
            Id = Guid.NewGuid(),
            UserId = _user.UserId!.Value,
            TokenAmount = tokenPackage.TokenAmount,
            Price = tokenPackage.Price,
            PaymentCode = PaymentConst.TokenPackagePrefix + Guid.NewGuid().ToString("N")[..30],
            IsPaid = false,
            TimeEnd = DateTimeOffset.Now.AddMinutes(3),
        };

        _context.UserTokenPackages.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        return new TokenPackageOrderDto
        {
            PaymentCode = order.PaymentCode,
            Amount = order.Price
        };
    }
}
