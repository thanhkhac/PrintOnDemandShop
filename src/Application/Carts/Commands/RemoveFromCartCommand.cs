using CleanArchitectureBase.Application.Common.Exceptions;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.Carts.Commands;

[Authorize]
public class RemoveFromCartCommand : IRequest<bool>
{
    public List<Guid>? CartItemIds { get; set; }
}

public class RemoveFromCartCommandValidator : AbstractValidator<RemoveFromCartCommand>
{
    public RemoveFromCartCommandValidator()
    {
        RuleFor(x => x.CartItemIds)
            .NotNull()
            .WithMessage("CartItemIds is required")
            .Must(ids => ids!.Any())
            .WithMessage("At least one CartItemId is required")
            .Must(ids => ids!.Count <= 50)
            .WithMessage("Cannot remove more than 50 items at once");
    }
}


public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RemoveFromCartCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<bool> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var idsToRemove = request.CartItemIds!.Distinct().ToList();

        var cartItems = await _context.CartItems
            .Where(ci => idsToRemove.Contains(ci.Id) && ci.CreatedBy == _user.UserId)
            .ToListAsync(cancellationToken);

        if (!cartItems.Any())
            throw new ErrorCodeException(ErrorCodes.CART_ITEM_NOT_FOUND);

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
