using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentDispatcher.Database.Migrations
{
    public partial class UniqueInmt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumericRequestKey",
                table: "DispatcherDBRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumericRequestKey",
                table: "DispatcherDBRequests");
        }
    }
}
