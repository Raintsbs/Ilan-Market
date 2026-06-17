using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdvertisementApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FullPackageB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "İsActive",
                table: "Categories",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "UptatedTime",
                table: "Categories",
                newName: "UpdatedTime");

            migrationBuilder.RenameColumn(
                name: "İsActive",
                table: "Advertisements",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "İmagePath",
                table: "Advertisements",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "UptatedTime",
                table: "Advertisements",
                newName: "UpdatedTime");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_UserId",
                table: "Advertisements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AspNetUsers_UserId",
                table: "Advertisements",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AspNetUsers_UserId",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_UserId",
                table: "Advertisements");

            migrationBuilder.RenameColumn(
                name: "UpdatedTime",
                table: "Categories",
                newName: "UptatedTime");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Categories",
                newName: "İsActive");

            migrationBuilder.RenameColumn(
                name: "UpdatedTime",
                table: "Advertisements",
                newName: "UptatedTime");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Advertisements",
                newName: "İsActive");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "Advertisements",
                newName: "İmagePath");
        }
    }
}
