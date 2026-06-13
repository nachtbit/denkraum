namespace Denkraum.Application.Abstractions;

public interface IExcelDocumentReader
{
    Task<IReadOnlyCollection<ExcelRowChunk>> ReadAsync(string filePath, CancellationToken cancellationToken);
}

public sealed record ExcelRowChunk(string SheetName, int RowNumber, string Content);
