# Matching Engine Reference

## Producer flow (Api)

`PlaceOrderCommand` handler:

1. Transaction: reserve wallet (buy) or holding (sell) + insert `Pending` order.
2. Commit.
3. Write place message to Incoming Order Channel.
4. Return to client.

`CancelOrderCommand`: persist cancel intent + enqueue cancel message (ordering with places).

## Partial index for book rebuild

`ix_orders_active_book` on `(symbol, side, price, created_at)` WHERE `status IN (0, 1)`.

## Outgoing consumers

- Persistence sink (if async path split)
- Notification sink → SignalR hub (market group per symbol, user group per user)
- Projection sink → Redis

## SignalR groups (Api)

| Group | Messages |
|-------|----------|
| Market per symbol | Order book, last price, trade tape |
| User per userId | Fills, cancels, balance updates |

MVP: single API instance, no SignalR backplane.

## Configuration (appsettings + Aspire env)

- Channel capacity
- Simulated liquidity distribution/refresh
- Initial virtual cash (related to placement validation in Application)

## Broker evolution

Channel abstraction should allow swapping in-process Channel for RabbitMQ without domain changes.
