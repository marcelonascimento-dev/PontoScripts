using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoScripts.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchToGlobalizacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Branch",
                table: "GlobalizacaoEntries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Branch",
                table: "GlobalizacaoEntries");
        }
    }
}
