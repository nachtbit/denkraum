using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace Denkraum.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase().Annotation("Npgsql:PostgresExtension:vector", ",,");

        migrationBuilder.CreateTable(
            name: "ChatSessions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_ChatSessions", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                FileName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                FilePath = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                IndexedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Documents", x => x.Id));

        migrationBuilder.CreateTable(
            name: "ChatMessages",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ChatMessages", x => x.Id);
                table.ForeignKey("FK_ChatMessages_ChatSessions_SessionId", x => x.SessionId, "ChatSessions", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "DocumentChunks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                SheetName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                RowNumber = table.Column<int>(type: "integer", nullable: false),
                Content = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentChunks", x => x.Id);
                table.ForeignKey("FK_DocumentChunks_Documents_DocumentId", x => x.DocumentId, "Documents", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Embeddings",
            columns: table => new
            {
                ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                Vector = table.Column<Vector>(type: "vector", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Embeddings", x => x.ChunkId);
                table.ForeignKey("FK_Embeddings_DocumentChunks_ChunkId", x => x.ChunkId, "DocumentChunks", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_ChatMessages_SessionId_CreatedAt", "ChatMessages", new[] { "SessionId", "CreatedAt" });
        migrationBuilder.CreateIndex("IX_DocumentChunks_DocumentId_SheetName_RowNumber", "DocumentChunks", new[] { "DocumentId", "SheetName", "RowNumber" });
        migrationBuilder.CreateIndex("IX_Documents_FilePath", "Documents", "FilePath", unique: true);
        migrationBuilder.Sql("""CREATE INDEX "IX_Embeddings_Vector" ON "Embeddings" USING ivfflat ("Vector" vector_cosine_ops);""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ChatMessages");
        migrationBuilder.DropTable("Embeddings");
        migrationBuilder.DropTable("ChatSessions");
        migrationBuilder.DropTable("DocumentChunks");
        migrationBuilder.DropTable("Documents");
    }
}
