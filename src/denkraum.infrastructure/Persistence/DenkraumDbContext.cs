using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Denkraum.Infrastructure.Persistence;

public sealed class DenkraumDbContext(DbContextOptions<DenkraumDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentChunk> DocumentChunks => Set<DocumentChunk>();
    public DbSet<Embedding> Embeddings => Set<Embedding>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DenkraumDbContext).Assembly);
    }
}
