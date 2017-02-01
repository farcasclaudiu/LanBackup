using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LanBackup.DataCore.Migrations
{
    public partial class MissedThings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BackupLog",
                table: "BackupLog");

            migrationBuilder.RenameTable(
                name: "BackupLog",
                newName: "Logs");

            migrationBuilder.RenameIndex(
                name: "IX_BackupLog_ConfigurationID",
                table: "Logs",
                newName: "IX_Logs_ConfigurationID");

            migrationBuilder.RenameIndex(
                name: "IX_BackupLog_ClientIP",
                table: "Logs",
                newName: "IX_Logs_ClientIP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Logs",
                table: "Logs",
                column: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Logs",
                table: "Logs");

            migrationBuilder.RenameTable(
                name: "Logs",
                newName: "BackupLog");

            migrationBuilder.RenameIndex(
                name: "IX_Logs_ConfigurationID",
                table: "BackupLog",
                newName: "IX_BackupLog_ConfigurationID");

            migrationBuilder.RenameIndex(
                name: "IX_Logs_ClientIP",
                table: "BackupLog",
                newName: "IX_BackupLog_ClientIP");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BackupLog",
                table: "BackupLog",
                column: "ID");
        }
    }
}
