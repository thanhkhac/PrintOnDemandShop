namespace CleanArchitectureBase.Application.ProductDesigns.Dtos;

public class ProductDesignDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    
    // Navigation properties
    public string? ProductName { get; set; }
    public string? ProductOptionValue { get; set; }
    public List<ProductDesignIconDto> Icons { get; set; } = new();
    public List<ProductDesignTemplateDto> Templates { get; set; } = new();
}

public class ProductDesignDetailDto : ProductDesignDto
{
    public ProductDto? Product { get; set; }
    public ProductOptionValueDto? ProductOptionValueDetail { get; set; }
}

public class ProductDesignIconDto
{
    public Guid Id { get; set; }
    public Guid ProductDesignId { get; set; }
    public string? ImageUrl { get; set; }
}

public class ProductDesignTemplateDto
{
    public Guid ProductDesignId { get; set; }
    public Guid TemplateId { get; set; }
    public string? DesignImageUrl { get; set; }
    public string? PrintAreaName { get; set; } // Snapshot from Template
    
    // Navigation properties from Template
    public string? TemplateImageUrl { get; set; } // From Template.ImageUrl
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
