namespace CleanArchitectureBase.Application.Common.Models;

public class FileStreamData
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public Stream? Data { get; set; }
}
