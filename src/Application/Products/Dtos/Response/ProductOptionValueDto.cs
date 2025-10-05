namespace CleanArchitectureBase.Application.Products.Dtos.ResponseDtos;

public class ProductOptionValueDto
{
    public Guid OptionValueId { get; set; }
    public string? Value { get; set; }
    public List<string> Images { get; set; } = new();
}
