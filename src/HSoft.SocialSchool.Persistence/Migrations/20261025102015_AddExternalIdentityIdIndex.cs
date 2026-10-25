using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSoft.SocialSchool.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalIdentityIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_ExternalIdentityId",
                table: "Users",
                column: "ExternalIdentityId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ExternalIdentityId",
                table: "Users");
        }
    }
}
