namespace CleanArchitectureBase.Application.Vouchers.Dtos;

public class VoucherDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? DiscountType { get; set; }
    public long? DiscountValue { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastModifiedAt { get; set; }
    
    // Navigation properties
    public List<VoucherProductDto> Products { get; set; } = new();
}

public class VoucherDetailDto : VoucherDto
{
    public List<ProductDto> ProductDetails { get; set; } = new();
}

public class VoucherProductDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public long BasePrice { get; set; }
}
