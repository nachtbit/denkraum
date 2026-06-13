namespace Denkraum.Domain.Entities;

public sealed class ChatSession
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ICollection<ChatMessage> Messages { get; } = new List<ChatMessage>();
}
