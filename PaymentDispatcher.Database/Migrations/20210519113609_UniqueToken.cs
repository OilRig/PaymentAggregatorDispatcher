using Microsoft.EntityFrameworkCore.Migrations;

namespace PaymentDispatcher.Database.Migrations
{
    public partial class UniqueToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueRequestToken",
                table: "DispatcherDBRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueRequestToken",
                table: "DispatcherDBRequests");
        }
    }
}
