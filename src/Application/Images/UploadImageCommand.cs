using CleanArchitectureBase.Application.Common.FileServices;
using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Images;

public class UploadImageCommand : IRequest<string>
{
    public FileStreamData? File { get; set; }
}

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull();
    }
}

public class UploadImageCommandHandler : IRequestHandler<UploadImageCommand, string>
{
    private readonly IImageService _imageService;

    public UploadImageCommandHandler(IImageService imageService)
    {
        _imageService = imageService;
    }

    public async Task<string> Handle(UploadImageCommand request, CancellationToken cancellationToken)
    {
        var url = await _imageService.UploadAsync(request.File!);

        return url;
    }
}
