using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kafgir.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TelegramUserId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AllowsWriteToPm = table.Column<bool>(type: "bit", nullable: false),
                    ChatId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramAccounts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO [TelegramAccounts]
                    ([UserId], [TelegramUserId], [Username], [FirstName], [LastName], [LanguageCode],
                     [AllowsWriteToPm], [ChatId], [CreatedAt], [LastSeenAt])
                SELECT
                    [Id],
                    [TelegramUserId],
                    [UserName],
                    [TelegramFirstName],
                    [TelegramLastName],
                    [TelegramLanguageCode],
                    [AllowsWriteToPm],
                    CAST([TelegramUserId] AS nvarchar(120)),
                    [CreatedAt],
                    [LastSeenAt]
                FROM [AspNetUsers]
                WHERE [TelegramUserId] IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAccounts_ChatId",
                table: "TelegramAccounts",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAccounts_TelegramUserId",
                table: "TelegramAccounts",
                column: "TelegramUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAccounts_UserId",
                table: "TelegramAccounts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramAccounts");
        }
    }
}
