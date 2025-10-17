using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;

using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Config cho Category
/// </summary>
public class CategoryConfig : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        // Self-reference cho category cha - con
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ nhiều–nhiều Category - Product thông qua bảng trung gian
        builder.HasMany(c => c.ProductCategories)
            .WithOne(pc => pc.Category)
            .HasForeignKey(pc => pc.CategoryId);
    }
}

/// <summary>
/// Config cho Product
/// </summary>
public class ProductConfig : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.BasePrice)
            .IsRequired();

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        // Quan hệ nhiều–nhiều Product - Category thông qua bảng trung gian
        builder.HasMany(p => p.ProductCategories)
            .WithOne(pc => pc.Product)
            .HasForeignKey(pc => pc.ProductId);

        // 1 Product → nhiều Option
        builder.HasMany(x => x.Options)
            .WithOne(o => o.Product)
            .HasForeignKey(o => o.ProductId);

        // 1 Product → nhiều Variant
        builder.HasMany(x => x.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId);
    }
}

/// <summary>
/// Config cho ProductCategory (bảng trung gian nhiều–nhiều)
/// </summary>
public class ProductCategoryConfig : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        // Composite key
        builder.HasKey(pc => new
        {
            pc.ProductId,
            pc.CategoryId
        });

        // Quan hệ với Product
        builder.HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId);

        // Quan hệ với Category
        builder.HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);
    }
}

/// <summary>
/// Config cho bảng ProductOption
/// </summary>
public class ProductOptionConfig : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("ProductOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Options)
            .HasForeignKey(x => x.ProductId);

        builder.HasMany(x => x.Values)
            .WithOne(v => v.ProductOption)
            .HasForeignKey(v => v.ProductOptionId);
    }
}

/// <summary>
/// Config cho bảng ProductOptionValue
/// </summary>
public class ProductOptionValueConfig : IEntityTypeConfiguration<ProductOptionValue>
{
    public void Configure(EntityTypeBuilder<ProductOptionValue> builder)
    {
        builder.ToTable("ProductOptionValues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.ProductOption)
            .WithMany(o => o.Values)
            .HasForeignKey(x => x.ProductOptionId);

        builder.HasMany(x => x.VariantValues)
            .WithOne(vv => vv.ProductOptionValue)
            .HasForeignKey(vv => vv.ProductOptionValueId);
    }
}

/// <summary>
/// Config cho bảng ProductVariant
/// </summary>
public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UnitPrice)
            .IsRequired();

        builder.Property(x => x.Stock)
            .IsRequired();

        builder.Property(x => x.Sku)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Sku);

        builder.HasOne(x => x.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(x => x.ProductId);

        // builder.HasMany(x => x.Images)
        //     .WithOne(i => i.ProductVariant)
        //     .HasForeignKey(i => i.ProductVariantId);

        builder.HasMany(x => x.VariantValues)
            .WithOne(vv => vv.ProductVariant)
            .HasForeignKey(vv => vv.ProductVariantId);
    }
}

// /// <summary>
// /// Config cho bảng ProductVariantImage
// /// </summary>
// public class ProductVariantImageConfig : IEntityTypeConfiguration<ProductVariantImage>
// {
//     public void Configure(EntityTypeBuilder<ProductVariantImage> builder)
//     {
//         builder.ToTable("ProductVariantImages");
//
//         builder.HasKey(x => x.Id);
//
//         builder.Property(x => x.ImageUrl)
//             .IsRequired()
//             .HasMaxLength(2000);
//
//         builder.Property(x => x.Order)
//             .IsRequired();
//
//         builder.HasOne(x => x.ProductVariant)
//             .WithMany(v => v.Images)
//             .HasForeignKey(x => x.ProductVariantId);
//     }
// }

/// <summary>
/// Config cho bảng ProductVariantValue (bảng nối nhiều-nhiều)
/// </summary>
public class ProductVariantValueConfig : IEntityTypeConfiguration<ProductVariantValue>
{
    public void Configure(EntityTypeBuilder<ProductVariantValue> builder)
    {
        builder.ToTable("ProductVariantValues");

        // Composite Key
        builder.HasKey(x => new
        {
            x.ProductVariantId,
            x.ProductOptionValueId
        });

        builder.HasOne(x => x.ProductVariant)
            .WithMany(v => v.VariantValues)
            .HasForeignKey(x => x.ProductVariantId);

        builder.HasOne(x => x.ProductOptionValue)
            .WithMany(v => v.VariantValues)
            .HasForeignKey(x => x.ProductOptionValueId);
    }
}

public class CartItemConfig : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).IsRequired();

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId);

        builder.HasOne(ci => ci.ProductDesign)
            .WithMany()
            .HasForeignKey(ci => ci.ProductDesignId);
    }
}

public class OrderConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasMaxLength(50);
        builder.Property(x => x.RecipientName).HasMaxLength(500);
        builder.Property(x => x.RecipientPhone).HasMaxLength(500);
        builder.Property(x => x.RecipientAddress).HasMaxLength(500);
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId);
    }
}

public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.VariantSku).HasMaxLength(100);
        builder.Property(x => x.VariantImageUrl).HasMaxLength(2000);

        builder.HasOne(oi => oi.ProductDesign)
            .WithMany()
            .HasForeignKey(oi => oi.ProductDesignId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId);
    }
}

public class VoucherConfig : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description).HasMaxLength(2000);

        builder.Property(x => x.DiscountValue);
        // builder.Property(x => x.MinOrderValue);
        // builder.Property(x => x.MaxDiscountAmount);
    }
}

public class TemplateConfig : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("Templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PrintAreaName).HasMaxLength(1000);

        builder.HasOne(t => t.ProductOptionValue)
            .WithMany()
            .HasForeignKey(t => t.ProductOptionValueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Product)
            .WithMany(p => p.Templates) // bạn cần thêm navigation vào Product
            .HasForeignKey(t => t.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
public class ProductVoucherConfiguration : IEntityTypeConfiguration<ProductVoucher>
{
    public void Configure(EntityTypeBuilder<ProductVoucher> builder)
    {
        // Table name (optional, nếu không EF sẽ lấy tên class)
        builder.ToTable("ProductVouchers");

        // Composite Key
        builder.HasKey(pv => new { pv.ProductId, pv.VoucherId });

        // Relationships
        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.ProductVouchers) // Product.ProductVouchers
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pv => pv.Voucher)
            .WithMany(v => v.ProductVouchers) // Voucher.ProductVouchers
            .HasForeignKey(pv => pv.VoucherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductOptionValueImageConfig : IEntityTypeConfiguration<ProductOptionValueImage>
{
    public void Configure(EntityTypeBuilder<ProductOptionValueImage> builder)
    {
        builder.ToTable("ProductOptionValueImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Order)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.HasOne(x => x.ProductOptionValue)
            .WithMany(v => v.Images) 
            .HasForeignKey(x => x.ProductOptionValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductDesignConfig : IEntityTypeConfiguration<ProductDesign>
{
    public void Configure(EntityTypeBuilder<ProductDesign> builder)
    {
        // Đặt tên bảng
        builder.ToTable("ProductDesigns");

        // Định nghĩa khóa chính
        builder.HasKey(x => x.Id);

        // Cấu hình các thuộc tính
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Quan hệ 1-n với Product
        builder.HasOne(x => x.Product)
            .WithMany() // Giả sử Product có navigation property ProductDesigns
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ với ProductOptionValue
        builder.HasOne(x => x.ProductOptionValue)
            .WithMany() // Không có navigation ngược từ ProductOptionValue
            .HasForeignKey(x => x.ProductOptionValueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quan hệ 1-n với ProductDesignIcons
        builder.HasMany(x => x.Icons)
            .WithOne(i => i.ProductDesign)
            .HasForeignKey(i => i.ProductDesignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kế thừa từ BaseAuditableEntity sẽ tự động có các trường như CreatedAt, UpdatedAt, v.v.
    }
}

public class ProductDesignIconsConfig : IEntityTypeConfiguration<ProductDesignIcons>
{
    public void Configure(EntityTypeBuilder<ProductDesignIcons> builder)
    {
        // Đặt tên bảng
        builder.ToTable("ProductDesignIcons");

        // Định nghĩa khóa chính
        builder.HasKey(x => x.Id);

        // Cấu hình các thuộc tính
        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(2000);

        // Quan hệ với ProductDesign
        builder.HasOne(x => x.ProductDesign)
            .WithMany(d => d.Icons)
            .HasForeignKey(x => x.ProductDesignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


public class ProductDesignTemplateConfig : IEntityTypeConfiguration<ProductDesignTemplate>
{
    public void Configure(EntityTypeBuilder<ProductDesignTemplate> builder)
    {
        // Đặt tên bảng
        builder.ToTable("ProductDesignTemplates");

        // Composite Key
        builder.HasKey(x => new { x.ProductDesignId, x.TemplateId });

        // Cấu hình các thuộc tính
        builder.Property(x => x.DesignImageUrl)
            .HasMaxLength(2000);

        builder.Property(x => x.PrintAreaName)
            .HasMaxLength(1000);

        // Quan hệ với ProductDesign
        builder.HasOne(x => x.ProductDesign)
            .WithMany() // Không có navigation ngược từ ProductDesign
            .HasForeignKey(x => x.ProductDesignId)
            .OnDelete(DeleteBehavior.Cascade);

        // Quan hệ với Template
        builder.HasOne(x => x.Template)
            .WithMany() // Giả sử Template có navigation property ProductDesignTemplates
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
