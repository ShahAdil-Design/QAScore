using AuditIQ.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditIQ.Infrastructure.Persistence.Configurations;

public class CommentLibraryItemConfiguration : IEntityTypeConfiguration<CommentLibraryItem>
{
    public void Configure(EntityTypeBuilder<CommentLibraryItem> builder)
    {
        builder.ToTable("CommentLibraryItems");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Text).HasMaxLength(500).IsRequired();
    }
}
