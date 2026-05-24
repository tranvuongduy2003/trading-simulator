using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Application.Portfolios.Queries;

public sealed class GetMyPortfolioQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IPortfolioReadRepository portfolioReadRepository) : QueryHandler<GetMyPortfolioQuery, PortfolioResponse>
{
    public override async Task<Result<PortfolioResponse>> Handle(
        GetMyPortfolioQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var portfolio = await portfolioReadRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (portfolio is null)
        {
            return Error.NotFound("PORTFOLIO_NOT_FOUND", "Portfolio was not found for the current user.");
        }

        var holdings = portfolio.Holdings
            .Select(holding => new HoldingDto(
                holding.Symbol,
                holding.TotalQuantity,
                holding.ReservedQuantity,
                holding.TotalQuantity - holding.ReservedQuantity,
                holding.AveragePrice))
            .ToList();

        return new PortfolioResponse(portfolio.PortfolioId, portfolio.UserId, holdings);
    }
}
