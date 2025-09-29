using System.ComponentModel.DataAnnotations;

namespace CleanArchitectureBase.Domain.Entities;

/// <summary>
/// Danh mục sản phẩm
/// </summary>
public class Category : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public List<Category> SubCategories { get; set; } = new();

    // Nhiều-nhiều: Category chứa nhiều Product thông qua bảng trung gian
    public List<ProductCategory> ProductCategories { get; set; } = new();

    public bool IsDeleted { get; set; }
}


/// <summary>
/// Bảng trung gian Product ↔ Category
/// </summary>
public class ProductCategory : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }
}


/// <summary>
/// Lưu trữ các sản phẩm
/// </summary>
public class Product : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public long BasePrice { get; set; }
    public string? ImageUrl { get; set; }

    // Nhiều-nhiều: Product nằm trong nhiều Category thông qua bảng trung gian
    public List<ProductCategory> ProductCategories { get; set; } = new();

    public List<ProductOption> Options { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public List<Template> Templates { get; set; } = new();

    public bool IsDeleted { get; set; }
}

/// <summary>
/// Lưu trữ các lựa chọn của sản phẩm: Giới hạn chỉ có màu, size
/// </summary>
public class ProductOption :  BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    
    public Product? Product { get; set; }
    public List<ProductOptionValue> Values { get; set; } = new();
}

/// <summary>
/// Lưu trữ các giá trị của các lựa chọn
/// </summary>
public class ProductOptionValue : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductOptionId { get; set; }
    public string? Value { get; set; }
    
    public ProductOption? ProductOption { get; set; }
    public List<ProductVariantValue> VariantValues { get; set; } = new();
    
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Các biến thể cuối cùng của sản phẩm
/// </summary>
public class ProductVariant : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public long Price { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
    public string? ImageUrl { get; set; }
    
    public Product? Product { get; set; }
    public List<ProductVariantImage> Images { get; set; } = new();
    public List<ProductVariantValue> VariantValues { get; set; } = new();
    
    public bool IsDeleted { get; set; }
}

/// <summary>
/// Bản mock up mẫu
/// </summary>
public class Template : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public string? PrintArea { get; set; }
    //Các trường thông tin cho mockup
    
    public ProductOptionValue? ProductOptionValue { get; set; }
    public Product? Product { get; set; }
}

/// <summary>
/// Bản thiết kế do người dùng tự thiết kế
/// </summary>
public class ProductDesign : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ProductOptionValueId { get; set; }
    public Guid TemplateId { get; set; }
    //Các trường thông tin cho mockup
}

/// <summary>
/// Các hình ảnh của biến thể
/// </summary>
public class ProductVariantImage : BaseEntity
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
    
    public ProductVariant? ProductVariant { get; set; }
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
    public Guid CartId { get; set; }
    public Guid ProductVariantId { get; set; }
    public int Quantity { get; set; }

    // Liên kết với bản thiết kế
    public Guid? ProductDesignId { get; set; }

    public ProductVariant? ProductVariant { get; set; }
    public ProductDesign? ProductDesign { get; set; }
}



public class Order : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
    public long TotalAmount { get; set; }
    public long SubTotal { get; set; }
    public Guid? VoucherId { get; set; }
    public string? VoucherCode { get; set; }
    public long Discount { get; set; }
    
    public string? ShippingAddress { get; set; }
    public string? PaymentMethod { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem : BaseEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }
    public Guid ProductVariantId { get; set; }
    public Guid? ProductDesignId { get; set; }    
    
    // Snapshot data
    public string Name { get; set; } = string.Empty;
    public string? VariantSku { get; set; }
    public string? VariantImageUrl { get; set; }

    public long UnitPrice { get; set; } 
    public int Quantity { get; set; }
    public long SubTotal { get; set; }
    
    public Order? Order { get; set; }
    public ProductDesign? ProductDesign { get; set; }

}


public class Voucher : BaseAuditableEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; 
    public string? Description { get; set; }

    public long? DiscountAmount { get; set; }   
    public long? DiscountPercent { get; set; }  

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int UsageLimit { get; set; }        
    public int UsedCount { get; set; }          
    public long? MinOrderValue { get; set; } 
    public long? MaxDiscountAmount { get; set; } 
 

    public bool IsActive => DateTime.UtcNow >= StartDate 
                            && DateTime.UtcNow <= EndDate 
                            && UsedCount < UsageLimit;
}




