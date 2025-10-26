using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.TokenPackages.Commands;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class DeleteTokenPackageCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteTokenPackageCommandValidator : AbstractValidator<DeleteTokenPackageCommand>
{
    public DeleteTokenPackageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("TokenPackage Id is required");
    }
}

public class DeleteTokenPackageCommandHandler : IRequestHandler<DeleteTokenPackageCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTokenPackageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTokenPackageCommand request, CancellationToken cancellationToken)
    {
        var tokenPackage = await _context.TokenPackages
            .FirstOrDefaultAsync(tp => tp.Id == request.Id && !tp.IsDeleted, cancellationToken);

        if (tokenPackage == null)
            throw new ErrorCodeException(ErrorCodes.TOKEN_PACKAGE_NOT_FOUND);

        // Soft delete
        tokenPackage.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
