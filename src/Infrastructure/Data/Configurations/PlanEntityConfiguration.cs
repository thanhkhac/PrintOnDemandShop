using CleanArchitectureBase.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;

public class TokenPackageConfiguration : IEntityTypeConfiguration<TokenPackage>
{
    public void Configure(EntityTypeBuilder<TokenPackage> builder)
    {
        builder.ToTable("TokenPackages");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenAmount)
            .IsRequired();

        builder.Property(t => t.Price)
            .IsRequired();
    }
}



public class UserTokenPackageConfiguration : IEntityTypeConfiguration<UserTokenPackage>
{
    public void Configure(EntityTypeBuilder<UserTokenPackage> builder)
    {
        builder.ToTable("UserTokenPackages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenAmount)
            .IsRequired();

        builder.Property(x => x.Price)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}


