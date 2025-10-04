using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Common.FileServices;

public interface IImageService
{
    Task<string> UploadAsync(FileStreamData file, string folder = "uploads");
}
