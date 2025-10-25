using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Application.Common.Security;
using CleanArchitectureBase.Domain.Constants;
using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.SampleImages;

[Authorize(Roles = Roles.Administrator + "," + Roles.Moderator)]
public class AddSampleImageCommand : IRequest<Guid>
{
    public string? ImageUrl { get; set; }
    // public string? Name { get; set; }
}

public class AddSampleImageCommandValidator : AbstractValidator<AddSampleImageCommand>
{
    public AddSampleImageCommandValidator()
    {
        RuleFor(x => x.ImageUrl).NotEmpty();
    }
}

public class AddSampleImageHandler : IRequestHandler<AddSampleImageCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddSampleImageHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddSampleImageCommand request, CancellationToken cancellationToken)
    {
        var newSI = new SampleImage
        {
            Id = Guid.NewGuid(),
            ImageUrl = request.ImageUrl
        };
        _context.SampleImages.Add(newSI);

        await _context.SaveChangesAsync(cancellationToken);

        return newSI.Id;
    }
}
