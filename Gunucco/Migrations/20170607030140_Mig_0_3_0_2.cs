using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Gunucco.Migrations
{
    public partial class Mig_0_3_0_2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimelineItem",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    ActionTargetId = table.Column<int>(nullable: true),
                    ListRangeValue = table.Column<int>(nullable: false, defaultValue: 2147483647)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    ServerPath = table.Column<string>(type: "varchar(256)", nullable: true),
                    TargetAction = table.Column<short>(nullable: false),
                    TargetActionValue = table.Column<short>(nullable: false, defaultValue: (short)101)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    TargetId = table.Column<int>(nullable: false),
                    TargetTypeValue = table.Column<short>(nullable: false, defaultValue: (short)101)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Updated = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimelineItem", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimelineItem");
        }
    }
}
