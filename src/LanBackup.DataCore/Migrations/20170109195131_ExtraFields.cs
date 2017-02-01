using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LanBackup.DataCore.Migrations
{
    public partial class ExtraFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ClientIP",
                table: "Backups",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Crontab",
                table: "Backups",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Backups_ClientIP",
                table: "Backups",
                column: "ClientIP");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Backups_ClientIP",
                table: "Backups");

            migrationBuilder.DropColumn(
                name: "Crontab",
                table: "Backups");

            migrationBuilder.AlterColumn<string>(
                name: "ClientIP",
                table: "Backups",
                nullable: false);
        }
    }
}
