using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LanBackup.DataCore.Migrations
{
    public partial class InitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Backups",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    ClientIP = table.Column<string>(nullable: false),
                    DestLanFolder = table.Column<string>(nullable: true),
                    DestPass = table.Column<string>(nullable: true),
                    DestUser = table.Column<string>(nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    SrcFolder = table.Column<string>(nullable: true),
                    SrcPass = table.Column<string>(nullable: true),
                    SrcUser = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Backups", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Backups");
        }
    }
}
