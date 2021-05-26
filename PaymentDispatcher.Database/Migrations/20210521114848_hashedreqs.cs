using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentDispatcher.Database.Migrations
{
    public partial class hashedreqs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestHashCode",
                table: "DispatcherDBRequests");

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
                    RequestSecretCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestId = table.Column<long>(type: "bigint", nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HashedPaymentRequests");

            migrationBuilder.DropColumn(
                name: "RequestSecretCode",
                table: "DispatcherDBRequests");

            migrationBuilder.AddColumn<int>(
                name: "RequestHashCode",
                table: "DispatcherDBRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
