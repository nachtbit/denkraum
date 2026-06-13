using Denkraum.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Denkraum.Infrastructure.Persistence.Configurations;

public sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.CreatedAt).IsRequired();
        builder.HasMany(session => session.Messages)
            .WithOne(message => message.Session)
            .HasForeignKey(message => message.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
