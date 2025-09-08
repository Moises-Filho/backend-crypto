using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CryptoPrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChange24h = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    PriceChangePercentage24h = table.Column<decimal>(type: "decimal(18,8)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoPrices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPrices_LastUpdated",
                table: "CryptoPrices",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPrices_Symbol",
                table: "CryptoPrices",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CryptoPrices");
        }
    }
}
