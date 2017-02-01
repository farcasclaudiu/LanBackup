using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LanBackup.DataCore.Migrations
{
    public partial class LogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BackupLog",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClientIP = table.Column<string>(nullable: true),
                    ConfigurationID = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Error = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Status = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupLog", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BackupLog_ClientIP",
                table: "BackupLog",
                column: "ClientIP");

            migrationBuilder.CreateIndex(
                name: "IX_BackupLog_ConfigurationID",
                table: "BackupLog",
                column: "ConfigurationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupLog");
        }
    }
}
