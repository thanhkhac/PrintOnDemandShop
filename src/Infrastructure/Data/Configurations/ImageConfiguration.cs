using CleanArchitectureBase.Domain.Entities;
using CleanArchitectureBase.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitectureBase.Infrastructure.Data.Configurations;

public class SampleImageConfiguration : IEntityTypeConfiguration<SampleImage>
{
    public void Configure(EntityTypeBuilder<SampleImage> builder)
    {
        builder.HasKey(u => u.Id);
    }
}
