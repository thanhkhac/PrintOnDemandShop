﻿using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;



public class AccountConfig : IEntityTypeConfiguration<UserAccount>
{

    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.Property(t => t.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(x => !x.IsDeleted);  
    }
}


public class UserConfig : IEntityTypeConfiguration<User>
{

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .HasMaxLength(500);
            
        builder.HasQueryFilter(x => !x.IsDeleted && !x.IsBanned);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(u => u.IsBanned)
            .HasDefaultValue(false);
            

        builder.Property(u => u.TokenCount)
            .HasDefaultValue(0);
                
        builder
            .HasOne<UserAccount>()
            .WithOne(account => account.User)
            .HasForeignKey<User>(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}



public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(x => x.UserAccountId).HasMaxLength(36);
        builder.Property(x => x.Id).HasMaxLength(36);
        builder.Property(x => x.Token).HasMaxLength(500);
        
        builder.HasQueryFilter(rt => rt.UserAccount != null && !rt.UserAccount.IsDeleted);
        
        builder.HasOne(rt => rt.UserAccount)
        .WithMany()
        .HasForeignKey(rt => rt.UserAccountId);
    }
}

public class TransactionConfigutation : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        
        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict); 
            
        builder.HasIndex(t => t.PaymentId).IsUnique();
    }
}
