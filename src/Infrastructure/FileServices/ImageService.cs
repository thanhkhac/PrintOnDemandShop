using CleanArchitectureBase.Application.Common.FileServices;
using CleanArchitectureBase.Application.Common.Models;
using Microsoft.AspNetCore.Hosting;

namespace CleanArchitectureBase.Infrastructure.FileServices;

public class ImageService : IImageService
{
    private readonly string _rootPath;
    
    public ImageService(IWebHostEnvironment env)
    {
        _rootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }
    
    public async Task<string> UploadAsync(FileStreamData file, string folder = "uploads")
    {
        if (file.Data == null || file.Data.Length == 0)
            throw new ArgumentException("File is empty");

        string uploadPath = Path.Combine(_rootPath, folder);
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await file.Data.CopyToAsync(stream);
        }

        return $"/{folder}/{fileName}";
    }}
