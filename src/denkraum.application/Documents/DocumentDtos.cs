namespace Denkraum.Application.Documents;

public sealed record DocumentDto(Guid Id, string FileName, string FilePath, DateTimeOffset IndexedAt, DateTimeOffset CreatedAt);
