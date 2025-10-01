using CleanArchitectureBase.Domain.Entities;

namespace CleanArchitectureBase.Application.Common.Interfaces;

public interface
    IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }
    
    DbSet<User> DomainUsers { get; }
    public DbSet<Transaction> Transactions { get; }
    
    
    DbSet<Category> Categories { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductOption> ProductOptions { get; }
    DbSet<ProductOptionValue> ProductOptionValues { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductVariantImage> ProductVariantImages { get; }
    DbSet<ProductVariantValue> ProductVariantValues { get; }
    
    DbSet<Template> Templates { get; }
    DbSet<ProductDesign> ProductDesigns { get; }
    
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    
    
    DbSet<Voucher> Vouchers { get; }
    DbSet<ProductVoucher> ProductVouchers { get; }
    
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
