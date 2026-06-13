namespace Denkraum.Domain.Entities;

public sealed class DocumentChunk
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    public required string SheetName { get; set; }
    public int RowNumber { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Embedding? Embedding { get; set; }
}
