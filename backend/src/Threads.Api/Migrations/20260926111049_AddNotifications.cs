using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Threads.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "social",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    TriggeredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    NotifiedUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: false
                    ),
                    SubjectKind = table.Column<string>(
                        type: "character varying(32)",
                        maxLength: 32,
                        nullable: true
                    ),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    SeenAtUtc = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.CheckConstraint(
                        "CK_Notifications_SeenAtUtc_NotBeforeTriggeredAtUtc",
                        "\"SeenAtUtc\" IS NULL OR \"SeenAtUtc\" >= \"TriggeredAtUtc\""
                    );
                    table.CheckConstraint(
                        "CK_Notifications_SubjectKind_SubjectId",
                        "(\"SubjectKind\" IS NULL AND \"SubjectId\" IS NULL) OR (\"SubjectKind\" IS NOT NULL AND \"SubjectId\" IS NOT NULL)"
                    );
                    table.CheckConstraint(
                        "CK_Notifications_TriggeredByUserId_NotNotifiedUserId",
                        "\"TriggeredByUserId\" <> \"NotifiedUserId\""
                    );
                    table.ForeignKey(
                        name: "FK_Notifications_Users_NotifiedUserId",
                        column: x => x.NotifiedUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_Notifications_Users_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotifiedUserId_TriggeredAtUtc_Id",
                schema: "social",
                table: "Notifications",
                columns: new[] { "NotifiedUserId", "TriggeredAtUtc", "Id" },
                descending: new[] { false, true, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_NotifiedUserId_Type_TriggeredAtUtc_Id",
                schema: "social",
                table: "Notifications",
                columns: new[] { "NotifiedUserId", "Type", "TriggeredAtUtc", "Id" },
                descending: new[] { false, false, true, true }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TriggeredByUserId",
                schema: "social",
                table: "Notifications",
                column: "TriggeredByUserId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Notifications", schema: "social");
        }
    }
}
