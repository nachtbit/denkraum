using System.Text;
using ClosedXML.Excel;
using Denkraum.Application.Abstractions;

namespace Denkraum.Infrastructure.Services;

public sealed class ExcelDocumentReader : IExcelDocumentReader
{
    public Task<IReadOnlyCollection<ExcelRowChunk>> ReadAsync(string filePath, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook(filePath);
        var chunks = new List<ExcelRowChunk>();
        var workbookName = Path.GetFileName(filePath);

        foreach (var worksheet in workbook.Worksheets)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = worksheet.RangeUsed();
            if (range is null)
            {
                continue;
            }

            var rows = range.RowsUsed().ToList();
            if (rows.Count == 0)
            {
                continue;
            }

            var headers = rows[0].CellsUsed()
                .Select((cell, index) => new { Index = index + 1, Name = Normalize(cell.GetFormattedString()) })
                .Where(header => !string.IsNullOrWhiteSpace(header.Name))
                .ToList();

            foreach (var row in rows.Skip(1))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var builder = new StringBuilder();
                builder.AppendLine($"Workbook: {workbookName}");
                builder.AppendLine($"Sheet: {worksheet.Name}");
                builder.AppendLine($"Row: {row.RowNumber()}");

                foreach (var header in headers)
                {
                    var value = Normalize(row.Cell(header.Index).GetFormattedString());
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        builder.AppendLine($"{header.Name}: {value}");
                    }
                }

                var content = builder.ToString().Trim();
                if (content.Contains(':', StringComparison.Ordinal))
                {
                    chunks.Add(new ExcelRowChunk(worksheet.Name, row.RowNumber(), content));
                }
            }
        }

        return Task.FromResult<IReadOnlyCollection<ExcelRowChunk>>(chunks);
    }

    private static string Normalize(string value) => string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
