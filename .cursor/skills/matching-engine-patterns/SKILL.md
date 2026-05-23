---
name: matching-engine-patterns
description: Guides Channel-based order intake, single-threaded matching loop, order book recovery, MatchingService usage, and match persistence for TradingSimulator.MatchingEngine. Use when implementing the matching worker, order book, simulated liquidity, channel adapters, or match-related integration tests.
---

# Matching Engine Patterns

Sources: `docs/TECHNICAL.md` §7–8, §16.4, §17.2; `docs/DATABASE.md` §10.2, §12.

## Goals

1. Fast API — persist + enqueue; do not block on match completion.
2. Deterministic matching — strict FIFO processing in one engine loop.
3. Backpressure — bounded incoming channel.

## Channel topology

| Channel | Bounded | Writer | Reader(s) |
|---------|---------|--------|-----------|
| Incoming Order | Yes (configurable) | Api (`PlaceOrder`, `CancelOrder` handlers) | MatchingEngine loop |
| Outgoing Trade Event | No | MatchingEngine | Persistence, SignalR projection, Redis projection |

Place and cancel share **incoming** channel to preserve submission order.

## Consumer loop (MatchingEngine host)

Single background loop:

1. Read from Incoming Order Channel.
2. **Place:** load order → `MatchingService` against in-memory book → zero or more trades.
3. **Cancel:** remove from book → update order status.
4. Core matching step is **single-threaded**; concurrency only at channel edges.

On shutdown: complete channel → drain remaining messages.

On startup: rebuild in-memory book from persisted open orders (`status IN (0,1)`); channel is ephemeral.

## MatchingService (Domain)

- Price-time priority (price first, then `created_at`).
- Limit buy ≥ best ask → match; limit sell ≤ best bid → match.
- Market orders consume liquidity; unfilled remainder → cancel per PRD/tests.
- Partial fills update both orders; execution price = **maker** price.
- No persistence inside service.

## After match (Application + Infrastructure)

Dispatch `ProcessMatchCommand` (or equivalent) inside **one transaction** per match step:

1. Update maker/taker orders (`filled_quantity`, `status`, `terminated_at` when terminal).
2. Insert `trades` row.
3. Update buyer wallet (debit total + reserved) and seller wallet (credit).
4. Update buyer holding (increment, weighted avg) and seller holding (decrement + release reserved).
5. Upsert `candlesticks` for affected buckets.
6. Commit → then write Redis projections (`orderbook:{symbol}:snapshot`, `trades:{symbol}:recent`, candle keys).
7. Publish to Outgoing Trade Event Channel for notifications.

Redis failure is non-fatal; Postgres is authoritative.

## Resilience

- Catch exceptions **per channel message**; log; reject order if still Pending; continue loop.
- Do not stop engine on single-order failure.

## Simulated liquidity

- Hosted inside MatchingEngine (not user traffic).
- Periodic limit orders around target price; `orders.is_simulated = true`.

## Pessimistic locks (narrow)

`SELECT … FOR UPDATE` on wallet/holding only inside brief match critical section when needed (`DATABASE.md` §9.3).

## Required unit tests

- Price-time priority
- Partial fills
- Market order liquidity consumption + remainder cancel
- Limit buy at/above best ask matches immediately
- Limit sell at/below best bid matches immediately

## Anti-patterns

- Matching on API request thread.
- Unbounded incoming channel.
- Skipping order-book rebuild on startup.
- Redis write in same transaction as Postgres.
- Multi-threaded access to in-memory book without synchronization.

## Reference

Channel and recovery detail: [matching-reference.md](matching-reference.md)
