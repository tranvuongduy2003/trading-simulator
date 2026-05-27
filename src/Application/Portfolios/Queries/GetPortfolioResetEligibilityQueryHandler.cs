using Microsoft.Extensions.Options;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Common;
using TradingSimulator.Application.Options;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Application.Portfolios.Queries;

public sealed class GetPortfolioResetEligibilityQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IPortfolioResetReadRepository portfolioResetReadRepository,
    IClock clock,
    IOptions<TradingOptions> tradingOptions)
    : QueryHandler<GetPortfolioResetEligibilityQuery, PortfolioResetEligibilityResponse>
{
    public override async Task<Result<PortfolioResetEligibilityResponse>> Handle(
        GetPortfolioResetEligibilityQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var latestResetAt = await portfolioResetReadRepository.GetLatestResetAtByUserIdAsync(
            userId.Value,
            cancellationToken);

        if (latestResetAt is null)
        {
            return new PortfolioResetEligibilityResponse(IsEligible: true, NextEligibleAt: null);
        }

        var nextEligibleAt = latestResetAt.Value.AddMinutes(
            tradingOptions.Value.PortfolioResetCooldownMinutes);
        var isEligible = clock.UtcNow >= nextEligibleAt;

        return new PortfolioResetEligibilityResponse(isEligible, nextEligibleAt);
    }
}
