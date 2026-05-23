# CQRS Reference

## Query return shapes (Contracts)

- Wallet: cash breakdown (total, reserved, available).
- Portfolio: holdings + valuation.
- Open orders / order history: paginated.
- Trade history: paginated; uses denormalized `buyer_user_id` / `seller_user_id`.
- Order book: top N bid/ask levels (Redis snapshot preferred).
- Recent trades: trade tape (Redis list preferred).
- Candlesticks: OHLCV by symbol + interval.

## Concurrency retry

- Tables with `row_version`: users, wallets, portfolios, orders.
- Retry policy: small bounded attempts + randomized backoff at command boundary.
- Exhausted retries → machine-readable error code in Problem Details (RFC 7807).

## Domain event dispatch

Collect events from aggregates during handler work; publish after UoW commit (in-process MVP). Consumers: notifications, projections — not in Domain.

## Login on command pipeline

`LoginUserCommand` stays on write pipeline due to session side effects even though it reads credentials.

## Testing expectations

Integration (Api + Testcontainers): register/login, place non-matching limit, matching pair, cancel, insufficient funds rejection.

Unit: handlers mocked repositories; domain rules tested in Domain.UnitTests.

## MediatR registration (Infrastructure or Application)

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(PlaceOrderCommand).Assembly));
services.AddValidatorsFromAssembly(typeof(PlaceOrderCommandValidator).Assembly);
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>)); // commands only filter
```

Adjust behavior registration to skip UoW for queries (marker interface or separate MediatR configuration).
