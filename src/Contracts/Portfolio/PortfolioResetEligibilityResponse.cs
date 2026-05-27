namespace TradingSimulator.Contracts.Portfolio;

public sealed record PortfolioResetEligibilityResponse(
    bool IsEligible,
    DateTimeOffset? NextEligibleAt);
