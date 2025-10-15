using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Tokens.Commands;

[Authorize]
public class DecreaseUserTokenCommand : IRequest<int>
{
}

public class DecreaseUserTokenCommandHandler : IRequestHandler<DecreaseUserTokenCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public DecreaseUserTokenCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }
    public async Task<int> Handle(DecreaseUserTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.DomainUsers.FirstOrDefaultAsync(x => x.Id == _user.UserId, cancellationToken);
        user!.TokenCount--;
        if (user.TokenCount >= 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        return (int)user.TokenCount;
    }
}
