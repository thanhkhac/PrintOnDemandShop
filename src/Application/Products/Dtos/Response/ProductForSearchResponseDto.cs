using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Products.Dtos.Response;

public class ProductForSearchResponseDto
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long MinPrice { get; set; }
    public long MaxPrice { get; set; }
    public CreatedByDto? CreatedBy { get; set; }
}
