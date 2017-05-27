using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_0_6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScopeValue",
                table: "UserSession",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGeneratedOnAdd", true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Chapter",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScopeValue",
                table: "UserSession");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Chapter");
        }
    }
}
