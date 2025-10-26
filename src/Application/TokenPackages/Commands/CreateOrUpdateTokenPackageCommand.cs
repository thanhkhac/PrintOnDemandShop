using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TokenPackages.Commands;

[Authorize(Roles = Domain.Constants.Roles.Administrator + "," + Domain.Constants.Roles.Moderator)]
public class CreateOrUpdateTokenPackageCommand : IRequest<Guid>
{
    public Guid? Id { get; set; }
    public int TokenAmount { get; set; }
    public int Price { get; set; }
}

public class CreateOrUpdateTokenPackageCommandValidator : AbstractValidator<CreateOrUpdateTokenPackageCommand>
{
    public CreateOrUpdateTokenPackageCommandValidator()
    {
        RuleFor(x => x.TokenAmount).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(2000);
    }
}

public class CreateOrUpdateTokenPackageCommandHandler : IRequestHandler<CreateOrUpdateTokenPackageCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrUpdateTokenPackageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrUpdateTokenPackageCommand request, CancellationToken cancellationToken)
    {
        TokenPackage? tokenPackage = null;
        if (request.Id.HasValue)
        {
            tokenPackage = _context.TokenPackages.FirstOrDefault(t => t.Id == request.Id.Value);
            if (tokenPackage == null)
                throw new ErrorCodeException(ErrorCodes.TOKEN_PACKAGE_NOT_FOUND);
            tokenPackage.Price = request.Price;
            tokenPackage.TokenAmount = request.TokenAmount;
        }
        else
        {
            tokenPackage = new TokenPackage()
            {
                Id = Guid.NewGuid(),
                TokenAmount = request.TokenAmount,
                Price = request.Price
            };
            _context.TokenPackages.Add(tokenPackage);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return tokenPackage.Id;
    }
}
