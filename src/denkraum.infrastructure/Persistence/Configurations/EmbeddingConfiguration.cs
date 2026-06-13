using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;

namespace Denkraum.Infrastructure.Persistence.Configurations;

public sealed class EmbeddingConfiguration : IEntityTypeConfiguration<Embedding>
{
    public void Configure(EntityTypeBuilder<Embedding> builder)
    {
        var converter = new ValueConverter<float[], Vector>(
            value => new Vector(value),
            value => value.ToArray());

        builder.ToTable("Embeddings");
        builder.HasKey(embedding => embedding.ChunkId);
        builder.Property(embedding => embedding.Vector)
            .HasConversion(converter)
            .HasColumnType("vector")
            .IsRequired();

        builder.HasOne(embedding => embedding.Chunk)
            .WithOne(chunk => chunk.Embedding)
            .HasForeignKey<Embedding>(embedding => embedding.ChunkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
