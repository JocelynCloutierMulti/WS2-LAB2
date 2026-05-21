using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FR_WS2_BaseLab.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Table already created via SQL script (Cours4_CategoryImages.sql)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryImages");
        }
    }
}
