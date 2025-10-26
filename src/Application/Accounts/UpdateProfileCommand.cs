using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;

namespace CleanArchitectureBase.Application.Accounts;

[Authorize]
public class UpdateProfileCommand : IRequest<Guid>
{
    public string? FullName { get; set; }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    public UpdateProfileCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public Task<Guid> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = _context.DomainUsers.First(x => x.Id == _user.UserId);
        user.FullName = request.FullName;
        _context.SaveChangesAsync(cancellationToken);
        return Task.FromResult(user.Id);
    }
}
