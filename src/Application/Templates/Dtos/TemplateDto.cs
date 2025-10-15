namespace CleanArchitectureBase.Application.Templates.Dtos;

public class TemplateDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string? PrintAreaName { get; set; }
    public string? ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    
    // Navigation properties
    public string? ProductName { get; set; }
    public string? ProductOptionName { get; set; }
    public string? ProductOptionValue { get; set; }
}

public class TemplateDetailDto : TemplateDto
{
    public ProductDto? Product { get; set; }
    public ProductOptionValueDto? ProductOptionValueDetail { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductOptionValueDto
{
    public Guid Id { get; set; }
    public string? Value { get; set; }
    public string? OptionName { get; set; }
}
