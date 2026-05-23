using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TradingSimulator.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialTradingSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "trading");

        migrationBuilder.CreateTable(
            name: "symbols",
            schema: "trading",
            columns: table => new
            {
                code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                tick_size = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0.01m),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_symbols", x => x.code);
                table.CheckConstraint("ck_symbols_tick_size_positive", "tick_size > 0");
            });

        migrationBuilder.CreateTable(
            name: "users",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                username = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "candlesticks",
            schema: "trading",
            columns: table => new
            {
                symbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                interval = table.Column<short>(type: "smallint", nullable: false),
                bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                open_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                high_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                low_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                close_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                volume = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                trade_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_candlesticks", x => new { x.symbol, x.interval, x.bucket_start });
                table.CheckConstraint("ck_candlesticks_high_low", "high_price >= low_price");
                table.CheckConstraint("ck_candlesticks_prices_positive", "open_price > 0 AND high_price > 0 AND low_price > 0 AND close_price > 0");
                table.CheckConstraint("ck_candlesticks_volume_non_negative", "volume >= 0");
                table.ForeignKey(
                    name: "FK_candlesticks_symbols_symbol",
                    column: x => x.symbol,
                    principalSchema: "trading",
                    principalTable: "symbols",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                side = table.Column<short>(type: "smallint", nullable: false),
                type = table.Column<short>(type: "smallint", nullable: false),
                price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                original_quantity = table.Column<long>(type: "bigint", nullable: false),
                filled_quantity = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                status = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                rejection_reason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                is_simulated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                terminated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                row_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.id);
                table.CheckConstraint("ck_orders_filled_le_original", "filled_quantity <= original_quantity");
                table.CheckConstraint("ck_orders_filled_non_negative", "filled_quantity >= 0");
                table.CheckConstraint("ck_orders_limit_has_price", "(type = 0 AND price IS NOT NULL AND price > 0) OR (type = 1 AND price IS NULL)");
                table.CheckConstraint("ck_orders_quantity_positive", "original_quantity > 0");
                table.CheckConstraint("ck_orders_side_range", "side IN (0, 1)");
                table.CheckConstraint("ck_orders_status_range", "status IN (0, 1, 2, 3, 4)");
                table.CheckConstraint("ck_orders_terminal_has_terminated_at", "(status IN (2, 3, 4)) = (terminated_at IS NOT NULL)");
                table.CheckConstraint("ck_orders_type_range", "type IN (0, 1)");
                table.ForeignKey(
                    name: "FK_orders_symbols_symbol",
                    column: x => x.symbol,
                    principalSchema: "trading",
                    principalTable: "symbols",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_orders_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "trading",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "portfolio_resets",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                reset_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_portfolio_resets", x => x.id);
                table.ForeignKey(
                    name: "FK_portfolio_resets_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "trading",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "portfolios",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_portfolios", x => x.id);
                table.ForeignKey(
                    name: "FK_portfolios_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "trading",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "user_sessions",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_sessions", x => x.id);
                table.ForeignKey(
                    name: "FK_user_sessions_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "trading",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "wallets",
            schema: "trading",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                total_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                reserved_balance = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "USD"),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                row_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_wallets", x => x.user_id);
                table.CheckConstraint("ck_wallets_reserved_le_total", "reserved_balance <= total_balance");
                table.CheckConstraint("ck_wallets_reserved_non_negative", "reserved_balance >= 0");
                table.CheckConstraint("ck_wallets_total_non_negative", "total_balance >= 0");
                table.ForeignKey(
                    name: "FK_wallets_users_user_id",
                    column: x => x.user_id,
                    principalSchema: "trading",
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "trades",
            schema: "trading",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                external_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                symbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                buy_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                sell_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                buyer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                seller_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                quantity = table.Column<long>(type: "bigint", nullable: false),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_trades", x => x.id);
                table.CheckConstraint("ck_trades_orders_distinct", "buy_order_id <> sell_order_id");
                table.CheckConstraint("ck_trades_price_positive", "price > 0");
                table.CheckConstraint("ck_trades_quantity_positive", "quantity > 0");
                table.CheckConstraint("ck_trades_users_distinct", "buyer_user_id <> seller_user_id");
                table.ForeignKey(
                    name: "FK_trades_orders_buy_order_id",
                    column: x => x.buy_order_id,
                    principalSchema: "trading",
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_trades_orders_sell_order_id",
                    column: x => x.sell_order_id,
                    principalSchema: "trading",
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_trades_symbols_symbol",
                    column: x => x.symbol,
                    principalSchema: "trading",
                    principalTable: "symbols",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "holdings",
            schema: "trading",
            columns: table => new
            {
                portfolio_id = table.Column<Guid>(type: "uuid", nullable: false),
                symbol = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                total_quantity = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                reserved_quantity = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                average_price = table.Column<decimal>(type: "numeric(18,4)", nullable: false, defaultValue: 0m),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_holdings", x => new { x.portfolio_id, x.symbol });
                table.CheckConstraint("ck_holdings_avg_price_non_negative", "average_price >= 0");
                table.CheckConstraint("ck_holdings_reserved_le_total", "reserved_quantity <= total_quantity");
                table.CheckConstraint("ck_holdings_reserved_non_negative", "reserved_quantity >= 0");
                table.CheckConstraint("ck_holdings_total_non_negative", "total_quantity >= 0");
                table.ForeignKey(
                    name: "FK_holdings_portfolios_portfolio_id",
                    column: x => x.portfolio_id,
                    principalSchema: "trading",
                    principalTable: "portfolios",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_holdings_symbols_symbol",
                    column: x => x.symbol,
                    principalSchema: "trading",
                    principalTable: "symbols",
                    principalColumn: "code",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.InsertData(
            schema: "trading",
            table: "symbols",
            columns: new[] { "code", "created_at", "is_active", "name", "tick_size" },
            values: new object[] { "AAPL", new DateTimeOffset(new DateTime(2026, 5, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "Apple Inc.", 0.01m });

        migrationBuilder.CreateIndex(
            name: "IX_holdings_symbol",
            schema: "trading",
            table: "holdings",
            column: "symbol");

        migrationBuilder.CreateIndex(
            name: "ix_orders_active_book",
            schema: "trading",
            table: "orders",
            columns: new[] { "symbol", "side", "price", "created_at" },
            filter: "status IN (0, 1)");

        migrationBuilder.CreateIndex(
            name: "ix_orders_terminated",
            schema: "trading",
            table: "orders",
            column: "terminated_at",
            descending: new bool[0],
            filter: "terminated_at IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "ix_orders_user_status",
            schema: "trading",
            table: "orders",
            columns: new[] { "user_id", "status", "created_at" },
            descending: new[] { false, false, true });

        migrationBuilder.CreateIndex(
            name: "ix_portfolio_resets_user_time",
            schema: "trading",
            table: "portfolio_resets",
            columns: new[] { "user_id", "reset_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ux_portfolios_user",
            schema: "trading",
            table: "portfolios",
            column: "user_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_trades_buy_order",
            schema: "trading",
            table: "trades",
            column: "buy_order_id");

        migrationBuilder.CreateIndex(
            name: "ix_trades_buyer_time",
            schema: "trading",
            table: "trades",
            columns: new[] { "buyer_user_id", "executed_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ix_trades_sell_order",
            schema: "trading",
            table: "trades",
            column: "sell_order_id");

        migrationBuilder.CreateIndex(
            name: "ix_trades_seller_time",
            schema: "trading",
            table: "trades",
            columns: new[] { "seller_user_id", "executed_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ix_trades_symbol_time",
            schema: "trading",
            table: "trades",
            columns: new[] { "symbol", "executed_at" },
            descending: new[] { false, true });

        migrationBuilder.CreateIndex(
            name: "ux_trades_external_id",
            schema: "trading",
            table: "trades",
            column: "external_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_user_sessions_expires",
            schema: "trading",
            table: "user_sessions",
            column: "expires_at");

        migrationBuilder.CreateIndex(
            name: "ix_user_sessions_user",
            schema: "trading",
            table: "user_sessions",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "ux_users_email",
            schema: "trading",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ux_users_username",
            schema: "trading",
            table: "users",
            column: "username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "candlesticks",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "holdings",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "portfolio_resets",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "trades",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "user_sessions",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "wallets",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "portfolios",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "orders",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "symbols",
            schema: "trading");

        migrationBuilder.DropTable(
            name: "users",
            schema: "trading");
    }
}
