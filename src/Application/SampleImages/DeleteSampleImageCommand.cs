using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;

namespace CleanArchitectureBase.Application.SampleImages;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class DeleteSampleImageCommand : IRequest<Guid>
{
    public Guid Id { get; set; }    
}


public class DeleteSampleImageCommandHandler : IRequestHandler<DeleteSampleImageCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    
    public DeleteSampleImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Guid> Handle(DeleteSampleImageCommand request, CancellationToken cancellationToken)
    {
        var entity = _context.SampleImages.Find(request.Id);
        if (entity != null)
        {
            _context.SampleImages.Remove(entity);
        }
        return Task.FromResult(request.Id);
    }
}
