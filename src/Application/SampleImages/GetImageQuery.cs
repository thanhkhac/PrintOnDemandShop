using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.SampleImages;

public class GetImageQuery : IRequest<List<string>>
{
    
}

public class AddSampleImageCommandHandler : IRequestHandler<GetImageQuery, List<string>>
{
    private readonly IApplicationDbContext _context;
    public AddSampleImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var data = await  _context.SampleImages.Where(x => x.ImageUrl != null).Select(x => x.ImageUrl!).ToListAsync();
        
        return data;
    }
}
