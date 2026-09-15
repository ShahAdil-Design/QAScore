using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class KudosConfiguration : IEntityTypeConfiguration<Kudos>
{
    public void Configure(EntityTypeBuilder<Kudos> builder)
    {
        builder.ToTable("Kudos");
        builder.HasKey(k => k.Id);
        builder.Property(k => k.Message).HasMaxLength(1000).IsRequired();

        builder.HasOne(k => k.FromUser)
            .WithMany()
            .HasForeignKey(k => k.FromUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(k => k.ToUser)
            .WithMany()
            .HasForeignKey(k => k.ToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(k => k.ToUserId);
        builder.HasIndex(k => k.CreatedAt);
    }
}
