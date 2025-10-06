namespace CleanArchitectureBase.Application.Carts.Dtos.Response;

public class CartItemResponseDto
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
    public Guid ProductVariantId { get; set; }
    public string? ProductVariantSku { get; set; }
    public long UnitPrice { get; set; }
    public int Stock { get; set; }
    public int Quantity { get; set; }
    public long TotalPrice { get; set; }
    public Guid? ProductDesignId { get; set; }
    public List<ProductVariantOptionDto> VariantOptions { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
}

