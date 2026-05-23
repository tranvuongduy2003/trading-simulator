# Product Requirements Document

**Product Name:** Real-time Stock Trading Simulator
**Document Version:** 1.0
**Status:** Draft
**Last Updated:** May 22, 2026
**Document Owner:** Product Owner

---

## 1. Document Control

| Field | Value |
|-------|-------|
| Product Name | Real-time Stock Trading Simulator |
| Product Code | RTSS |
| Document Type | Product Requirements Document (PRD) |
| Version | 1.0 |
| Status | Draft |
| Release Target | MVP (v1.0) |

### 1.1 Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 0.1 | 2026-05-22 | Product Owner | Initial draft |
| 1.0 | 2026-05-22 | Product Owner | Approved for development |

---

## 2. Executive Summary

The Real-time Stock Trading Simulator is a single-symbol trading platform that enables users to practice equity trading in a risk-free environment using virtual currency. Users can place buy and sell orders against a live, simulated order book, observe real-time price movements, and track their portfolio performance.

The platform replicates the core mechanics of a real exchange — including limit and market orders, price-time priority matching, partial fills, and order cancellation — while removing real financial risk. The MVP focuses on a single tradable instrument (AAPL) to provide depth of experience over breadth of inventory.

---

## 3. Product Vision and Goals

### 3.1 Product Vision

To provide an authentic, real-time stock trading experience that allows users to understand how modern equity exchanges operate and to develop trading intuition without exposure to financial loss.

### 3.2 Product Goals

1. Deliver a believable simulation of a real exchange order book with accurate matching behavior.
2. Provide real-time market data updates with sub-second latency from order placement to UI reflection.
3. Enable users to manage a virtual portfolio and track performance over time.
4. Maintain a clean, intuitive interface that does not overwhelm new users while offering enough depth for experienced ones.

### 3.3 Non-Goals (Out of Scope for MVP)

- Multiple tradable symbols (the MVP supports only AAPL).
- Real money or integration with real brokerage accounts.
- Advanced order types (stop-loss, trailing stop, iceberg, etc.).
- Mobile native applications (web-only for MVP).
- Social features (chat, leaderboards, copy trading).
- Options, futures, or derivatives.
- News feed or fundamental analysis tools.
- Multi-currency support (USD only).

---

## 4. Target Users and Personas

### 4.1 Primary Persona: The Aspiring Trader

- **Profile:** Adults aged 22–40 with interest in financial markets but limited or no real trading experience.
- **Goals:** Understand how exchanges work, practice placing different order types, build confidence before risking real capital.
- **Pain Points:** Real brokerages require money to start, paper trading accounts at real brokers often have delayed or unrealistic execution, educational content is theoretical and disconnected from execution mechanics.

### 4.2 Secondary Persona: The Curious Learner

- **Profile:** Students, finance enthusiasts, or career-switchers exploring trading as a discipline.
- **Goals:** Learn terminology (bid, ask, spread, order book depth), see how orders interact in real time.
- **Pain Points:** Most learning resources are passive (videos, articles); they want hands-on exploration.

### 4.3 User Assumptions

- Users have basic familiarity with the concept of stocks and trading.
- Users access the platform via a modern desktop or laptop web browser.
- Users are not required to deposit real money; they receive virtual starting capital on registration.

---

## 5. User Stories and Use Cases

### 5.1 Epic: Account Management

| ID | User Story | Priority |
|----|-----------|----------|
| US-01 | As a new user, I want to register an account so that I can start trading. | Must |
| US-02 | As a returning user, I want to log in securely so that I can access my portfolio. | Must |
| US-03 | As a user, I want to see my virtual cash balance so that I know how much I can trade with. | Must |
| US-04 | As a user, I want to reset my portfolio so that I can start fresh after a poor performance. | Should |

### 5.2 Epic: Market Data

| ID | User Story | Priority |
|----|-----------|----------|
| US-05 | As a user, I want to see the current best bid and ask prices so that I know the market state. | Must |
| US-06 | As a user, I want to see the live order book (depth) so that I can gauge market liquidity. | Must |
| US-07 | As a user, I want to see the latest trades (trade tape) so that I can observe market activity. | Must |
| US-08 | As a user, I want to see a price chart with candlestick history so that I can analyze price movement. | Should |
| US-09 | As a user, I want all market data to update in real time without manually refreshing the page. | Must |

### 5.3 Epic: Order Placement

| ID | User Story | Priority |
|----|-----------|----------|
| US-10 | As a user, I want to place a limit buy order with a specified price and quantity. | Must |
| US-11 | As a user, I want to place a limit sell order with a specified price and quantity. | Must |
| US-12 | As a user, I want to place a market buy order to execute immediately at the best available price. | Must |
| US-13 | As a user, I want to place a market sell order to execute immediately at the best available price. | Must |
| US-14 | As a user, I want to see a confirmation before submitting an order so that I avoid accidental trades. | Should |
| US-15 | As a user, I want to be prevented from placing an order that exceeds my available balance or holdings. | Must |

### 5.4 Epic: Order Management

| ID | User Story | Priority |
|----|-----------|----------|
| US-16 | As a user, I want to view all my open (unfilled or partially filled) orders. | Must |
| US-17 | As a user, I want to view my order history including filled and cancelled orders. | Must |
| US-18 | As a user, I want to cancel an open order so that I can withdraw it from the market. | Must |
| US-19 | As a user, I want to be notified in real time when my order is filled or partially filled. | Must |

### 5.5 Epic: Portfolio Management

| ID | User Story | Priority |
|----|-----------|----------|
| US-20 | As a user, I want to see my current holdings, including quantity and average purchase price. | Must |
| US-21 | As a user, I want to see the unrealized profit and loss for my holdings based on the current market price. | Must |
| US-22 | As a user, I want to see my trade history so that I can review past activity. | Must |
| US-23 | As a user, I want to see my total portfolio value (cash plus holdings). | Should |

---

## 6. Functional Requirements

### 6.1 User Account

**FR-1.1 Registration**
- Users must be able to register with a unique username, email address, and password.
- Email format must be validated.
- Password must meet a minimum complexity requirement (length, character variety).
- Username must be unique across the system.

**FR-1.2 Authentication**
- Users must be able to log in using their credentials.
- Sessions must persist across page reloads until the user logs out or the session expires.
- Users must be able to log out at any time.

**FR-1.3 Initial Capital**
- Every newly registered user receives a default virtual cash balance of USD 100,000.
- This is a non-real, non-redeemable virtual currency used only within the platform.

**FR-1.4 Portfolio Reset**
- A user may reset their portfolio at most once every 24 hours.
- Reset returns the user to the initial state: USD 100,000 cash, zero holdings, all open orders cancelled, trade history cleared.

### 6.2 Market Data

**FR-2.1 Order Book Display**
- The platform must display the top N (configurable, default 10) price levels on both the bid and ask sides.
- For each level, show: price, aggregated quantity, and number of orders at that level.
- The order book must update in real time as orders are placed, cancelled, or matched.

**FR-2.2 Last Trade Price**
- The platform must display the most recent execution price, prominently visible at all times.
- The price should update immediately upon any new trade.

**FR-2.3 Trade Tape**
- A scrolling feed of the most recent trades (default last 50), each showing price, quantity, and timestamp.
- New trades appear at the top of the feed in real time.

**FR-2.4 Candlestick Chart**
- A chart showing historical price aggregated into time intervals (default: 1-minute candles).
- Each candle shows open, high, low, and close (OHLC) and traded volume.
- The chart must update as new trades occur.
- Users may switch between time intervals (1m, 5m, 15m, 1h).

**FR-2.5 Spread and Mid-Price**
- The platform must display the current bid-ask spread (absolute and as a percentage).
- The mid-price (average of best bid and best ask) must be visible.

### 6.3 Order Placement

**FR-3.1 Limit Order**
- Users must be able to place limit orders by specifying: side (buy or sell), price, and quantity.
- Price must be a positive decimal with up to 4 decimal places.
- Quantity must be a positive integer.
- The order is added to the order book if not immediately matchable.

**FR-3.2 Market Order**
- Users must be able to place market orders by specifying: side (buy or sell) and quantity.
- Market orders execute immediately against the best available prices.
- If the order cannot be fully filled due to insufficient liquidity, the remaining quantity is cancelled (immediate-or-cancel behavior).

**FR-3.3 Order Validation**
- A buy order requires sufficient available cash balance to cover `price × quantity` (for limit orders) or an estimated worst-case cost (for market orders, based on current asks).
- A sell order requires sufficient available holdings of the symbol.
- Funds and holdings are reserved at order placement and only released upon execution or cancellation.

**FR-3.4 Order Confirmation**
- Before final submission, the user is shown a confirmation summary including: side, type, price, quantity, and estimated cost or proceeds.

**FR-3.5 Order Submission Feedback**
- After submission, the user receives an immediate response with the order ID and status.
- If the order is rejected, the reason must be clearly displayed.

### 6.4 Order Matching

**FR-4.1 Matching Priority**
- Orders are matched using price-time priority: best price first, and within the same price, earliest order first (FIFO).

**FR-4.2 Partial Fills**
- An order may be partially filled if the opposing side has insufficient quantity at acceptable prices.
- The unfilled remainder of a limit order remains in the order book.
- The unfilled remainder of a market order is cancelled.

**FR-4.3 Trade Execution**
- When two orders match, a trade is created and recorded with: buy order ID, sell order ID, execution price, quantity, and timestamp.
- The execution price is the price of the resting order (the one already on the book).
- Both users' portfolios and balances are updated atomically with the trade.

### 6.5 Order Management

**FR-5.1 View Open Orders**
- Users must be able to view all their open orders (status: Pending or Partially Filled).
- Each open order displays: order ID, side, type, price, original quantity, filled quantity, remaining quantity, status, and timestamp.

**FR-5.2 View Order History**
- Users must be able to view their past orders (status: Filled, Cancelled).
- The history must be paginated with a default page size of 25 and support filtering by date range and status.

**FR-5.3 Cancel Order**
- Users may cancel any open order (Pending or Partially Filled).
- Cancellation must be reflected in the order book in real time.
- Reserved funds or holdings tied to the cancelled portion are released back to the user's available balance.

**FR-5.4 Order Notifications**
- The user receives a real-time notification in the UI when:
  - An order is filled (fully or partially).
  - An order is cancelled (by the user or by the system).
  - An order is rejected.

### 6.6 Portfolio

**FR-6.1 Holdings Display**
- The platform must show the user's current holdings: symbol, quantity, average purchase price, current market price, and unrealized profit/loss.

**FR-6.2 Cash Balance Display**
- The platform must show the user's total cash, reserved cash (tied up in open buy orders), and available cash.

**FR-6.3 Portfolio Valuation**
- Total portfolio value = available cash + reserved cash + (holdings quantity × current market price).
- This value updates in real time as the market price changes.

**FR-6.4 Trade History**
- Users may view all trades they participated in, showing: timestamp, side, price, quantity, and counterparty role (filled as buyer or seller).
- The history must be paginated and filterable by date range.

---

## 7. Non-Functional Requirements

### 7.1 Performance

| Metric | Target |
|--------|--------|
| Order placement response time (p95) | ≤ 100 ms |
| Order matching latency (p99) | ≤ 50 ms |
| Real-time market data update latency | ≤ 500 ms from trade execution to UI |
| Order throughput | ≥ 1,000 orders per second |

### 7.2 Reliability

- The platform must handle service restarts without losing committed order state.
- In-flight orders at the time of a restart must either complete or be safely recoverable.
- No double-execution of orders, even under concurrent submission.

### 7.3 Data Integrity

- A user's cash balance must never go negative.
- A user's holdings quantity must never go negative.
- The sum of all cash in the system must remain constant (no money created or destroyed by the platform).
- The sum of all holdings across users equals the total traded volume on each side.

### 7.4 Usability

- A new user must be able to register and place their first order within 3 minutes of arriving at the landing page.
- All primary actions (place order, cancel order, view portfolio) must be reachable within 2 clicks from the main trading view.
- Error messages must be clear and actionable.

### 7.5 Accessibility

- The interface must support keyboard navigation for all primary actions.
- Color choices must not be the sole means of conveying information (e.g., green/red must be accompanied by labels or icons).

### 7.6 Browser Support

- The platform must function correctly on the current and previous major versions of Chrome, Firefox, Safari, and Edge.

---

## 8. User Interface Requirements

### 8.1 Layout

The main trading view consists of:

- **Top bar:** Logo, current symbol (AAPL), last trade price, daily change, user menu.
- **Left panel:** Order book (depth view with bids and asks).
- **Center panel:** Price chart (candlestick) with timeframe selector.
- **Right panel:** Order placement form with tabs for limit and market orders.
- **Bottom panel:** Tabbed view containing Open Orders, Order History, Trade History, and Holdings.

### 8.2 Visual Design Principles

- Clean, minimal, and information-dense without being cluttered.
- Bid side displayed in green tones; ask side in red tones.
- Numeric data should use a monospace or tabular font for alignment.
- Real-time updates should be visually subtle (no jarring animations) but noticeable.

### 8.3 Responsive Behavior

- The primary trading view is designed for desktop (1280px and wider).
- A simplified, vertically stacked view is acceptable for tablet (≥ 768px).
- Mobile phone support is not required for MVP.

---

## 9. Success Metrics

### 9.1 Product Metrics (Post-Launch)

| Metric | Target (3 months post-launch) |
|--------|-------------------------------|
| Total registered users | 500 |
| Weekly active users | 150 |
| Orders placed per active user per week | ≥ 20 |
| Average session duration | ≥ 10 minutes |
| Day-7 retention | ≥ 30% |
| Day-30 retention | ≥ 15% |

### 9.2 Quality Metrics

| Metric | Target |
|--------|--------|
| Order matching correctness (audited) | 100% |
| Balance integrity violations | 0 |
| User-reported critical bugs per week | ≤ 1 |

---

## 10. Release Plan

### 10.1 MVP (v1.0)

**Scope:** All Must-have user stories listed in Section 5, all functional requirements in Section 6, single symbol (AAPL).

**Acceptance Criteria:**
- A user can register, log in, place limit and market orders on both sides, see their orders matched in real time, view their portfolio, and cancel open orders.
- All Must-have user stories pass acceptance testing.
- All performance targets in Section 7.1 are met under nominal load.

### 10.2 Future Releases (Post-MVP)

The following features are explicitly out of scope for MVP but are candidates for future releases. They are listed here for product roadmap visibility only.

**v1.1 — Quality of Life**
- Order modification (cancel-replace) without losing time priority on price-unchanged modifications.
- Configurable starting capital.
- Dark mode and light mode.

**v1.2 — Expanded Trading**
- Multiple tradable symbols.
- Stop-loss and stop-limit order types.

**v2.0 — Engagement**
- Leaderboards by portfolio performance.
- Daily and weekly trading challenges.

---

## 11. Assumptions and Dependencies

### 11.1 Assumptions

- Users will accept virtual currency as a substitute for real money for the purpose of learning.
- The single-symbol limitation is acceptable for the MVP.
- Simulated market activity (from a price generator or seeded liquidity) is sufficient to make the order book feel alive; real market data feeds are not required.

### 11.2 Dependencies

- A mechanism for generating simulated market activity (background liquidity orders) so that users have counterparties when no other real users are active.

---

## 12. Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Order book feels empty due to low user count | High | High | Implement simulated liquidity providers that place realistic orders. |
| Users disengage after initial novelty wears off | Medium | High | Introduce performance tracking and reset feature to encourage re-engagement. |
| User confusion with order types | Medium | Medium | Provide in-context tooltips and an optional first-time walkthrough. |
| Race conditions cause incorrect balances | Low | Critical | Strict invariant enforcement and reconciliation auditing. |
| Real-time updates feel laggy | Medium | High | Define and monitor latency budgets; degrade gracefully if needed. |

---

## 13. Glossary

| Term | Definition |
|------|------------|
| Order Book | A list of all open buy (bid) and sell (ask) orders for a symbol, sorted by price. |
| Bid | A buy order; the price a buyer is willing to pay. |
| Ask | A sell order; the price a seller is willing to accept. |
| Spread | The difference between the best ask and the best bid. |
| Limit Order | An order to buy or sell at a specific price or better. |
| Market Order | An order to buy or sell immediately at the best available price. |
| Partial Fill | An execution that fills only part of an order's quantity. |
| Price-Time Priority | The rule that orders are matched first by best price, then by earliest submission time. |
| Trade | A completed match between a buy order and a sell order. |
| Trade Tape | A real-time feed of executed trades. |
| Holding | A quantity of a symbol owned by a user. |
| Unrealized P&L | The profit or loss on current holdings based on the current market price, not yet realized through a sale. |
| Reserved Balance | Cash committed to open buy orders and unavailable for new orders. |

---

---

## Companion documents

| Document | Role |
|----------|------|
| [`TECHNICAL.md`](TECHNICAL.md) | Architecture, domain model, API, matching, configuration |
| [`DATABASE.md`](DATABASE.md) | PostgreSQL schema, Redis projections |
| [`TRACEABILITY.md`](TRACEABILITY.md) | Maps user stories and FRs to technical sections |

*End of Document*
