using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DecisionHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "decision_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dilemma = table.Column<string>(type: "text", nullable: false),
                    square = table.Column<string>(type: "jsonb", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: true),
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_decision_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    telegram_payment_charge_id = table.Column<string>(type: "text", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    premium_days = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usage_counters",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_kind = table.Column<short>(type: "smallint", nullable: false),
                    period_key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usage_counters", x => new { x.user_id, x.period_kind, x.period_key });
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    telegram_id = table.Column<long>(type: "bigint", nullable: true),
                    anon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    locale = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    is_premium = table.Column<bool>(type: "boolean", nullable: false),
                    premium_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    merged_into_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_decision_sessions_user_id",
                table: "decision_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_telegram_payment_charge_id",
                table: "payments",
                column: "telegram_payment_charge_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_user_id",
                table: "payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_anon_id",
                table: "users",
                column: "anon_id",
                unique: true,
                filter: "anon_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_telegram_id",
                table: "users",
                column: "telegram_id",
                unique: true,
                filter: "telegram_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decision_sessions");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "usage_counters");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
