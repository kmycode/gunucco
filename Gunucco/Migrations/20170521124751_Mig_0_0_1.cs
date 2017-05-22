using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentId",
                table: "BookPermission",
                newName: "TargetId");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "BookPermission");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "BookPermission");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TargetId",
                table: "BookPermission",
                newName: "ContentId");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "BookPermission",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "BookPermission",
                nullable: true);
        }
    }
}
