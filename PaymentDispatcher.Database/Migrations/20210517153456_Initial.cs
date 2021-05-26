using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentDispatcher.Database.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregatorTokenMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregatorToken = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AggregatorAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregatorTokenMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DispatcherDBRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueAggregatorToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Intent = table.Column<byte>(type: "tinyint", nullable: false),
                    Method = table.Column<short>(type: "smallint", nullable: false),
                    RequestHashCode = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatcherDBRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AggregatorTokenMaps_AggregatorAddress_AggregatorToken",
                table: "AggregatorTokenMaps",
                columns: new[] { "AggregatorAddress", "AggregatorToken" },
                unique: true,
                filter: "[AggregatorAddress] IS NOT NULL AND [AggregatorToken] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregatorTokenMaps");

            migrationBuilder.DropTable(
                name: "DispatcherDBRequests");
        }
    }
}
