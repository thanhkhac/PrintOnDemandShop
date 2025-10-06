
namespace CleanArchitectureBase.Application.Products.Dtos.Response;

public class ProductDetailResponseDto
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long BasePrice { get; set; }
    public CategoryDto? Category { get; set; }
    public List<ProductOptionDto> Options { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
}
