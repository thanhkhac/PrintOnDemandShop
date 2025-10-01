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
    
    public string? Value { get; set; }
    public bool IsDeleted { get; set; }
}


public class ProductVariant : BaseEntity
{
    public Guid Id { get; set; }
    
    public Guid ProductId { get; set; }
    
    public Product? Product { get; set; }
   
    public List<ProductVariantImage> Images { get; set; } = new();
    public List<ProductVariantValue> VariantValues { get; set; } = new();

    public long UnitPrice { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
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

    public Product? Product { get; set; }
    public ProductOptionValue? ProductOptionValue { get; set; }

    public string? PrintArea { get; set; }
    //Các trường thông tin cho mockup   
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
 
    public ProductVariant? ProductVariant { get; set; }
 
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
}



public class Order : BaseAuditableEntity
{
    public Guid Id { get; set; }

    public List<OrderItem> Items { get; set; } = new();

    public DateTimeOffset OrderDate { get; set; }
    public string? Status { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientAddress { get; set; }
    public string? PaymentMethod { get; set; }
    
    public long SubTotal { get; set; }
    public long TotalAmount { get; set; }
    public long DiscountAmount { get; set; }
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
    public long? DiscountAmount { get; set; }   
    public long? DiscountPercent { get; set; }  
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int UsageLimit { get; set; }        
    public int UsedCount { get; set; }          
    public long? MinOrderValue { get; set; } 
    public long? MaxDiscountAmount { get; set; } 
    public bool IsActive { get; set; }
}

public class ProductVoucher : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid VoucherId { get; set; }

    public Product? Product { get; set; }
    public Voucher? Voucher { get; set; }
}




