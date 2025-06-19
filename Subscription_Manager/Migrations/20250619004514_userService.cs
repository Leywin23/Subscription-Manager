using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription_Manager.Migrations
{
    /// <inheritdoc />
    public partial class userService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Frequency",
                table: "Subscriptions",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyCost",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "YearlyCost",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonthlyCost",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "YearlyCost",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Frequency",
                table: "Subscriptions",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
