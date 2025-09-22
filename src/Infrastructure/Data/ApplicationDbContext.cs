using System.Reflection;
using CleanArchitectureBase.Application.Common.Interfaces;
using CleanArchitectureBase.Domain.Common;
using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> DomainUsers => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // public override DbSet<ApplicationRole> Roles { get; set; }
    // public override DbSet<ApplicationUserClaim> UserClaims { get; set; }
    // public override DbSet<ApplicationUserLogin> UserLogins { get; set; }
    // public override DbSet<ApplicationUserToken> UserTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
