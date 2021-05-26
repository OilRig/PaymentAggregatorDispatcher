using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentDispatcher.Database.Migrations
{
    public partial class SomeMigr : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HashedPaymentRequests");

            migrationBuilder.DropColumn(
                name: "DataJson",
                table: "DispatcherDBRequests");

            migrationBuilder.DropColumn(
                name: "FinishDate",
                table: "DispatcherDBRequests");

            migrationBuilder.DropColumn(
                name: "NumericRequestKey",
                table: "DispatcherDBRequests");

            migrationBuilder.DropColumn(
                name: "RequestSecretCode",
                table: "DispatcherDBRequests");

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "DispatcherDBRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<short>(
                name: "Channel",
                table: "DispatcherDBRequests",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "DispatcherDBRequests");

            migrationBuilder.DropColumn(
                name: "Channel",
                table: "DispatcherDBRequests");

            migrationBuilder.AddColumn<string>(
                name: "DataJson",
                table: "DispatcherDBRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishDate",
                table: "DispatcherDBRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "NumericRequestKey",
                table: "DispatcherDBRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RequestSecretCode",
                table: "DispatcherDBRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "HashedPaymentRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregatorToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    RequestId = table.Column<long>(type: "bigint", nullable: false),
                    RequestSecretCode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashedPaymentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HashedPaymentRequests_DispatcherDBRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "DispatcherDBRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HashedPaymentRequests_RequestId",
                table: "HashedPaymentRequests",
                column: "RequestId",
                unique: true);
        }
    }
}
