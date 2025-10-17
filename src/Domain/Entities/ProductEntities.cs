using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureBase.Domain.Entities;

public class Category : BaseAuditableEntity
{
    public Guid Id { get; set; }
    
    public Guid? ParentCategoryId { get; set; }
    
    public Category? ParentCategory { get; set; }

    public List<Category> SubCategories { get; set; } = new();
    public List<ProductCategory> ProductCategories { get; set; } = new();

    public string? Name { get; set; }
    public bool IsDeleted { get; set; }
}


public class ProductCategory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid CategoryId { get; set; }

    public Product? Product { get; set; }
    public Category? Category { get; set; }
}


public class Product : BaseAuditableEntity
{
    public Guid Id { get; set; }
    
    public List<ProductCategory> ProductCategories { get; set; } = new();
    public List<ProductOption> Options { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public List<Template> Templates { get; set; } = new();
    public List<ProductVoucher> ProductVouchers { get; set; } = new();
    
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long BasePrice { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
}

public class ProductOption :  BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    
    public Product? Product { get; set; }
    
    public List<ProductOptionValue> Values { get; set; } = new();
    
    public string? Name { get; set; }
}


public class ProductOptionValue : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductOptionId { get; set; }
    
    public ProductOption? ProductOption { get; set; }
    
    public List<ProductVariantValue> VariantValues { get; set; } = new();
    public List<ProductOptionValueImage> Images { get; set; } = new(); 

    public string? Value { get; set; }
    public bool IsDeleted { get; set; }
}


public class ProductVariant : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    
    public Product? Product { get; set; }
    public string? ImageUrl { get; set; }
    // public List<ProductVariantImage> Images { get; set; } = new();
    public List<ProductVariantValue> VariantValues { get; set; } = new();

    public long UnitPrice { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
    public bool IsDeleted { get; set; }
}


// /// <summary>
// /// Các hình ảnh của biến thể
// /// </summary>
// public class ProductVariantImage : BaseEntity
// {
//     public Guid Id { get; set; }
//     
//     public Guid ProductVariantId { get; set; }
//  
//     public ProductVariant? ProductVariant { get; set; }
//  
//     public string? ImageUrl { get; set; }
//     public int Order { get; set; }
//     public bool IsDeleted { get; set; }  
// }



public class ProductOptionValueImage : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductOptionValueId { get; set; }
    
    public ProductOptionValue? ProductOptionValue { get; set; }
    
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
    public bool IsDeleted { get; set; }
}




/// <summary>
/// Các giá trị lựa chọn của biến thể
/// </summary>
public class ProductVariantValue : BaseEntity
{
    public Guid ProductVariantId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    
    public ProductVariant? ProductVariant { get; set; }
    public ProductOptionValue? ProductOptionValue { get; set; }
}

public class CartItem : BaseAuditableEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductVariantId { get; set; }
    public Guid? ProductDesignId { get; set; }

    public ProductVariant? ProductVariant { get; set; }
    public ProductDesign? ProductDesign { get; set; }

    public int Quantity { get; set; }
    
    public Guid? ProductId { get; set; }
}


/// <summary>
/// Bản mock up mẫu
/// </summary>
public class Template : BaseAuditableEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }

    public Product? Product { get; set; }
    public ProductOptionValue? ProductOptionValue { get; set; }

    public string? PrintAreaName { get; set; }
    //Các trường thông tin cho mockup   
    public string? ImageUrl { get; set; }
    
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Bản thiết kế do người dùng tự thiết kế
/// </summary>
public class ProductDesign : BaseAuditableEntity
{
    public Guid Id { get; set; }
 
    public Guid ProductOptionValueId { get; set; }
    public ProductOptionValue ? ProductOptionValue { get; set; }
    public Guid ProductId { get; set; } // tham chiếu sản phẩm gốc
    public Product? Product { get; set; }
    
    public string? Name { get; set; } // ví dụ "Thiết kế áo Tết 2025"
    
    public List<ProductDesignIcons> Icons { get; set; } = new();
    public bool IsDeleted { get; set; }
}    

public class ProductDesignIcons : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductDesignId { get; set; }
    public ProductDesign? ProductDesign { get; set; }
    
    public string? ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
}

public class ProductDesignTemplate : BaseEntity
{
    public Guid ProductDesignId { get; set; }
    public Guid TemplateId { get; set; }

    public ProductDesign? ProductDesign { get; set; }
    public Template? Template { get; set; }

    // Các trường riêng của bản thiết kế trên mockup này:
    public string? DesignImageUrl { get; set; } // ảnh thiết kế thực tế gắn lên template
    /// <summary>
    /// Snapshot của Template
    /// </summary>
    public string? PrintAreaName { get; set; }
    public bool IsDeleted { get; set; }
}


public class Order : BaseAuditableEntity
{
    public Guid Id { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public DateTimeOffset OrderDate { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientAddress { get; set; }
    public string? PaymentMethod { get; set; }
    
    // Tổng số tiền khi chưa tính giảm giá
    public long SubTotal { get; set; }
    // Tổng số tiền sau khi đã giảm giá
    public long TotalAmount { get; set; }
    // Tổng số tiền được giảm
    public long DiscountAmount { get; set; }
    
    public string? UserFeedback { get; set; }
    public int? Rating { get; set; }
    public string? PaymentCode { get; set; }
    
    public string? AdminNote { get; set; }
}

public class OrderItem : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid? ProductDesignId { get; set; }    
    public Guid? VoucherId { get; set; }
    
    public Order? Order { get; set; }
    public ProductDesign? ProductDesign { get; set; }
    public ProductVariant? ProductVariant { get; set; }
    public Voucher? Voucher { get; set; } 
    
    public string Name { get; set; } = string.Empty;
    public string? VariantSku { get; set; }
    public string? VariantImageUrl { get; set; }

    public long UnitPrice { get; set; } 
    public int Quantity { get; set; }
    
    public long SubTotal { get; set; }
    public long DiscountAmount { get; set; }
    public long TotalAmount { get; set; }
    public string? VoucherCode { get; set; }
    public long? VoucherDiscountAmount { get; set; }
    public long? VoucherDiscountPercent { get; set; }
}


public class Voucher : BaseAuditableEntity
{
    public Guid Id { get; set; }

    public List<ProductVoucher> ProductVouchers { get; set; } = new();

    public string Code { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public string? DiscountType { get; set; }
    public long? DiscountValue { get; set; }   
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int UsedCount { get; set; }           
    public bool IsActive { get; set; }
}

public class ProductVoucher : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid VoucherId { get; set; }

    public Product? Product { get; set; }
    public Voucher? Voucher { get; set; }
    
    // ReSharper disable once InconsistentNaming
    public string? _TempFieldToDisableJunctionAble { get; set; }
}
