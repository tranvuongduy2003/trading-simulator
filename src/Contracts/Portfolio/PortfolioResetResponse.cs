namespace TradingSimulator.Contracts.Portfolio;

public sealed record PortfolioResetResponse(
    DateTimeOffset ResetAt,
    DateTimeOffset NextEligibleAt,
    PortfolioResetWalletSnapshot Wallet);
