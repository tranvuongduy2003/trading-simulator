using Microsoft.Extensions.Options;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Portfolios;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Common;
using TradingSimulator.Application.Options;
using TradingSimulator.Contracts.Portfolio;

namespace TradingSimulator.Application.Portfolios.Commands;

public sealed class ResetPortfolioCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IPortfolioResetWriteRepository portfolioResetWriteRepository,
    IResetInFlightGuard resetInFlightGuard,
    IClock clock,
    IOptions<TradingOptions> tradingOptions)
    : CommandHandler<ResetPortfolioCommand, PortfolioResetResponse>
{
    public override async Task<Result<PortfolioResetResponse>> Handle(
        ResetPortfolioCommand command,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        if (!resetInFlightGuard.TryBegin(userId.Value))
        {
            return PortfolioResetErrors.ResetInProgress;
        }

        try
        {
            var resetAt = clock.UtcNow;
            var wallet = await portfolioResetWriteRepository.ResetForUserAsync(
                userId.Value,
                tradingOptions.Value.InitialVirtualCash,
                resetAt,
                cancellationToken);

            if (wallet is null)
            {
                return PortfolioResetErrors.WalletNotFound;
            }

            var nextEligibleAt = resetAt.AddMinutes(tradingOptions.Value.PortfolioResetCooldownMinutes);

            return new PortfolioResetResponse(
                resetAt,
                nextEligibleAt,
                new PortfolioResetWalletSnapshot(
                    wallet.TotalBalance,
                    wallet.ReservedBalance,
                    wallet.AvailableBalance,
                    wallet.Currency));
        }
        finally
        {
            resetInFlightGuard.End(userId.Value);
        }
    }
}
