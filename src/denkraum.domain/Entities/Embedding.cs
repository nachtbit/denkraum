namespace Denkraum.Domain.Entities;

public sealed class Embedding
{
    public Guid ChunkId { get; set; }
    public DocumentChunk Chunk { get; set; } = null!;
    public required float[] Vector { get; set; }
}
