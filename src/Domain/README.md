# TradingSimulator.Domain

Pure domain layer (no framework references). See `docs/TECHNICAL.md` §5 and `.cursor/skills/trading-domain-rules/`.

## Layout

| Folder | Contents (planned) |
|--------|-------------------|
| `Users/` | `User`, `Wallet` aggregate |
| `Portfolios/` | `Portfolio`, `Holding` aggregate |
| `Orders/` | `Order`, enums, state machine |
| `Trades/` | `Trade` aggregate |
| `Common/` | Value objects, typed IDs (`Money`, `Price`, `Quantity`, …) |
| `Events/` | Domain events |
| `Services/` | `OrderValidationService`, `MatchingService` |
| `Exceptions/` | Typed domain exceptions |

No implementation files yet — structure only.
