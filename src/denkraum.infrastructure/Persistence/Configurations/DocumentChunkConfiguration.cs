using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denkraum.Infrastructure.Persistence.Configurations;

public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> builder)
    {
        builder.ToTable("DocumentChunks");
        builder.HasKey(chunk => chunk.Id);
        builder.Property(chunk => chunk.SheetName).HasMaxLength(512).IsRequired();
        builder.Property(chunk => chunk.RowNumber).IsRequired();
        builder.Property(chunk => chunk.Content).IsRequired();
        builder.Property(chunk => chunk.CreatedAt).IsRequired();
        builder.HasIndex(chunk => new { chunk.DocumentId, chunk.SheetName, chunk.RowNumber });
    }
}
