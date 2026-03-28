using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PontoScripts.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalizacaoEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", nullable: false),
                    TraducaoPtBR = table.Column<string>(type: "TEXT", nullable: false),
                    TraducaoEnUS = table.Column<string>(type: "TEXT", nullable: false),
                    TraducaoEsES = table.Column<string>(type: "TEXT", nullable: false),
                    AtributoAdicional = table.Column<string>(type: "TEXT", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalizacaoEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScriptAlteracaoEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroOS = table.Column<string>(type: "TEXT", nullable: true),
                    Branch = table.Column<string>(type: "TEXT", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    ScriptSql = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoPor = table.Column<string>(type: "TEXT", nullable: false),
                    OrdemExecucao = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScriptAlteracaoEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Versoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataGeracao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersaoGlobalizacoes",
                columns: table => new
                {
                    VersaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    GlobalizacaoEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersaoGlobalizacoes", x => new { x.VersaoId, x.GlobalizacaoEntryId });
                    table.ForeignKey(
                        name: "FK_VersaoGlobalizacoes_GlobalizacaoEntries_GlobalizacaoEntryId",
                        column: x => x.GlobalizacaoEntryId,
                        principalTable: "GlobalizacaoEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VersaoGlobalizacoes_Versoes_VersaoId",
                        column: x => x.VersaoId,
                        principalTable: "Versoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VersaoScripts",
                columns: table => new
                {
                    VersaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ScriptAlteracaoEntryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersaoScripts", x => new { x.VersaoId, x.ScriptAlteracaoEntryId });
                    table.ForeignKey(
                        name: "FK_VersaoScripts_ScriptAlteracaoEntries_ScriptAlteracaoEntryId",
                        column: x => x.ScriptAlteracaoEntryId,
                        principalTable: "ScriptAlteracaoEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VersaoScripts_Versoes_VersaoId",
                        column: x => x.VersaoId,
                        principalTable: "Versoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlobalizacaoEntries_Tipo_Mensagem",
                table: "GlobalizacaoEntries",
                columns: new[] { "Tipo", "Mensagem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VersaoGlobalizacoes_GlobalizacaoEntryId",
                table: "VersaoGlobalizacoes",
                column: "GlobalizacaoEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_VersaoScripts_ScriptAlteracaoEntryId",
                table: "VersaoScripts",
                column: "ScriptAlteracaoEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VersaoGlobalizacoes");

            migrationBuilder.DropTable(
                name: "VersaoScripts");

            migrationBuilder.DropTable(
                name: "GlobalizacaoEntries");

            migrationBuilder.DropTable(
                name: "ScriptAlteracaoEntries");

            migrationBuilder.DropTable(
                name: "Versoes");
        }
    }
}
