using System.Reflection;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Common;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CleanArchitectureBase.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<UserAccount,
    ApplicationRole,
    Guid,
    ApplicationUserClaim,
    ApplicationUserRole,
    ApplicationUserLogin,
    ApplicationRoleClaim,
    ApplicationUserToken
>, IApplicationDbContext
{
    // private IDbContextTransaction? _currentTransaction;

    // public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    // {
    //     _currentTransaction ??= await Database.BeginTransactionAsync(cancellationToken);
    //     return _currentTransaction;
    // }
    //
    // public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    // {
    //     if (_currentTransaction != null)
    //     {
    //         await _currentTransaction.CommitAsync(cancellationToken);
    //         await _currentTransaction.DisposeAsync();
    //         _currentTransaction = null;
    //     }
    // }
    //
    // public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    // {
    //     if (_currentTransaction != null)
    //     {
    //         await _currentTransaction.RollbackAsync(cancellationToken);
    //         await _currentTransaction.DisposeAsync();
    //         _currentTransaction = null;
    //     }
    // }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionValue> ProductOptionValues => Set<ProductOptionValue>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    // public DbSet<ProductVariantImage> ProductVariantImages => Set<ProductVariantImage>();
    public DbSet<ProductVariantValue> ProductVariantValues => Set<ProductVariantValue>();
    public DbSet<ProductOptionValueImage> ProductOptionValueImages  => Set<ProductOptionValueImage>();


    public DbSet<Template> Templates => Set<Template>();
    public DbSet<ProductDesign> ProductDesigns => Set<ProductDesign>();
    public DbSet<ProductDesignIcons> ProductDesignIcons => Set<ProductDesignIcons>();
    public DbSet<ProductDesignTemplate> ProductDesignTemplates => Set<ProductDesignTemplate>();

    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    
    
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<ProductVoucher> ProductVouchers => Set<ProductVoucher>();
    public DbSet<TokenPackage> TokenPackages => Set<TokenPackage>();
    public DbSet<UserTokenPackage> UserTokenPackages => Set<UserTokenPackage>();

    // public override DbSet<ApplicationRole> Roles { get; set; }
    // public override DbSet<ApplicationUserClaim> UserClaims { get; set; }
    // public override DbSet<ApplicationUserLogin> UserLogins { get; set; }
    // public override DbSet<ApplicationUserToken> UserTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(CleanArchitectureBase.Domain.Common.BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType)
                    .HasOne(typeof(User), nameof(BaseAuditableEntity.CreatedByUser))
                    .WithMany()
                    .HasForeignKey(nameof(BaseAuditableEntity.CreatedBy))
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
}
