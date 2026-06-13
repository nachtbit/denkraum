namespace Denkraum.Domain.Entities;

public sealed class Document
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string FilePath { get; set; }
    public DateTimeOffset IndexedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<DocumentChunk> Chunks { get; } = new List<DocumentChunk>();
}
