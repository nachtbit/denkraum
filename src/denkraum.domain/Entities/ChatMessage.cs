namespace Denkraum.Domain.Entities;

public sealed class ChatMessage
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public ChatSession Session { get; set; } = null!;
    public required string Role { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
