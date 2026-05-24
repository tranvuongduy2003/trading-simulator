namespace TradingSimulator.Contracts.Portfolio;

public sealed record PortfolioResponse(
    Guid PortfolioId,
    Guid UserId,
    IReadOnlyList<HoldingDto> Holdings);
