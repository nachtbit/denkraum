using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denkraum.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.FileName).HasMaxLength(512).IsRequired();
        builder.Property(document => document.FilePath).HasMaxLength(2_048).IsRequired();
        builder.Property(document => document.IndexedAt).IsRequired();
        builder.Property(document => document.CreatedAt).IsRequired();
        builder.HasIndex(document => document.FilePath).IsUnique();
        builder.HasMany(document => document.Chunks)
            .WithOne(chunk => chunk.Document)
            .HasForeignKey(chunk => chunk.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
