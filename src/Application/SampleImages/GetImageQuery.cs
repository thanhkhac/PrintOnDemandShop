using CleanArchitectureBase.Application.Common.Interfaces;

namespace CleanArchitectureBase.Application.SampleImages;

public class SampleImageDto
{
    public Guid SampleImageId { get; set; }
    public string? ImageUrl { get; set; }
}

public class GetImageQuery : IRequest<List<SampleImageDto>>
{

}

public class AddSampleImageCommandHandler : IRequestHandler<GetImageQuery, List<SampleImageDto>>
{
    private readonly IApplicationDbContext _context;
    public AddSampleImageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SampleImageDto>> Handle(GetImageQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.SampleImages.Where(x => x.ImageUrl != null)
            .Select(x =>
                new SampleImageDto
                {
                    SampleImageId = x.Id,
                    ImageUrl = x.ImageUrl!
                }
            ).ToListAsync();

        return data;
    }
}
