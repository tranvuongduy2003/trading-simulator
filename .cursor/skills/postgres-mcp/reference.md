# Postgres MCP — query reference

Starter **read-only** queries for the trading-simulator. Adjust limits and filters as needed.

## Schema discovery

```sql
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_schema = 'trading'
ORDER BY table_name;
```

```sql
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'trading' AND table_name = 'orders'
ORDER BY ordinal_position;
```

## Symbols & reference data

```sql
SELECT id, ticker, name, is_active
FROM trading.symbols
ORDER BY ticker;
```

## User / wallet / portfolio

```sql
SELECT u.id, u.username, w.cash_balance, w.row_version
FROM trading.users u
JOIN trading.wallets w ON w.user_id = u.id
WHERE u.username = $1;  -- bind via literal in MCP: WHERE u.username = 'demo'
```

```sql
SELECT p.id, p.user_id, h.symbol_id, h.quantity, h.average_cost
FROM trading.portfolios p
LEFT JOIN trading.holdings h ON h.portfolio_id = p.id
WHERE p.user_id = '00000000-0000-0000-0000-000000000000';
```

## Orders (active book)

```sql
SELECT id, symbol_id, side, order_type, status, price, quantity, filled_quantity, created_at
FROM trading.orders
WHERE symbol_id = (SELECT id FROM trading.symbols WHERE ticker = 'AAPL')
  AND status IN (0, 1)
ORDER BY
  CASE side WHEN 0 THEN price END DESC,
  CASE side WHEN 1 THEN price END ASC,
  created_at;
```

## Trades (append-only)

```sql
SELECT id, symbol_id, buy_order_id, sell_order_id, price, quantity, executed_at
FROM trading.trades
ORDER BY executed_at DESC
LIMIT 20;
```

## Sessions & resets

```sql
SELECT id, user_id, expires_at, created_at
FROM trading.user_sessions
WHERE user_id = '00000000-0000-0000-0000-000000000000'
ORDER BY created_at DESC
LIMIT 5;
```

```sql
SELECT id, portfolio_id, reset_at
FROM trading.portfolio_resets
ORDER BY reset_at DESC
LIMIT 10;
```

## Diagnostics

```sql
SELECT status, COUNT(*) AS cnt
FROM trading.orders
GROUP BY status
ORDER BY status;
```

```sql
EXPLAIN (ANALYZE, BUFFERS)
SELECT *
FROM trading.orders
WHERE symbol_id = (SELECT id FROM trading.symbols WHERE ticker = 'AAPL')
  AND status IN (0, 1);
```

## EF migration history

```sql
SELECT migration_id, product_version
FROM trading."__EFMigrationsHistory"
ORDER BY migration_id;
```
