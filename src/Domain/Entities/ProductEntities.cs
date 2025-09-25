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





