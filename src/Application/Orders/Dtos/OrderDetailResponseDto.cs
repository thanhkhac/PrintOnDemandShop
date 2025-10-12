using CleanArchitectureBase.Application.Common.Models;

namespace CleanArchitectureBase.Application.Orders.Dtos;

public class OrderDetailResponseDto
{
    public Guid OrderId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public string? Status { get; set; }

    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientAddress { get; set; }

    public string? PaymentMethod { get; set; }

    public long SubTotal { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }

    public List<OrderItemResponseDto> Items { get; set; } = new();
    public CreatedByDto? CreatedBy { get; set; }
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid? ProductDesignId { get; set; }
    public Guid? VoucherId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? VariantSku { get; set; }
    public string? ImageUrl { get; set; }

    public long UnitPrice { get; set; }
    public int Quantity { get; set; }

    public long SubTotal { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }

    public string? VoucherCode { get; set; }
    public long? VoucherDiscountAmount { get; set; }
    public long? VoucherDiscountPercent { get; set; }
}
