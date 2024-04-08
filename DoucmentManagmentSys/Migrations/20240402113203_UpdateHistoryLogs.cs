﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHistoryLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Document_idId = table.Column<int>(type: "int", nullable: false),
                    Document_idFileName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_HistoryLogs_Documents_Document_idId_Document_idFileName",
                        columns: x => new { x.Document_idId, x.Document_idFileName },
                        principalTable: "Documents",
                        principalColumns: new[] { "Id", "FileName" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryActions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    historyLogid = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryActions", x => x.id);
                    table.ForeignKey(
                        name: "FK_HistoryActions_HistoryLogs_historyLogid",
                        column: x => x.historyLogid,
                        principalTable: "HistoryLogs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryActions_historyLogid",
                table: "HistoryActions",
                column: "historyLogid");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryLogs_Document_idId_Document_idFileName",
                table: "HistoryLogs",
                columns: new[] { "Document_idId", "Document_idFileName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryActions");

            migrationBuilder.DropTable(
                name: "HistoryLogs");
        }
    }
}