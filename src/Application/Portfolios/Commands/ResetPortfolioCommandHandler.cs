using Microsoft.Extensions.Options;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Abstractions.Portfolios;
using TradingSimulator.Application.Abstractions.Realtime;
using TradingSimulator.Application.Abstractions.Services;
using TradingSimulator.Application.Common;
using TradingSimulator.Application.Options;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Realtime;

namespace TradingSimulator.Application.Portfolios.Commands;

public sealed class ResetPortfolioCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IPortfolioResetReadRepository portfolioResetReadRepository,
    IPortfolioResetWriteRepository portfolioResetWriteRepository,
    IRealtimeNotificationPublisher realtimeNotificationPublisher,
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

        var latestResetAt = await portfolioResetReadRepository.GetLatestResetAtByUserIdAsync(
            userId.Value,
            cancellationToken);

        if (latestResetAt is not null)
        {
            var nextEligibleAt = latestResetAt.Value.AddMinutes(
                tradingOptions.Value.PortfolioResetCooldownMinutes);

            if (clock.UtcNow < nextEligibleAt)
            {
                return PortfolioResetErrors.CooldownActive(nextEligibleAt);
            }
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

            foreach (var cancelledOrder in wallet.CancelledOrders)
            {
                await realtimeNotificationPublisher.NotifyOrderCancellationAsync(
                    userId.Value.Value,
                    new OrderCancellationNotificationMessage(cancelledOrder.OrderId, cancelledOrder.Symbol, resetAt),
                    cancellationToken);
            }

            foreach (var symbol in wallet.CancelledOrders.Select(cancelledOrder => cancelledOrder.Symbol).Distinct())
            {
                await realtimeNotificationPublisher.PublishOrderBookUpdatedAsync(
                    symbol,
                    new OrderBookUpdatedMessage(symbol, [], [], resetAt),
                    cancellationToken);
            }

            await realtimeNotificationPublisher.NotifyBalanceUpdatedAsync(
                userId.Value.Value,
                new BalanceUpdatedMessage(
                    userId.Value.Value,
                    wallet.TotalBalance,
                    wallet.ReservedBalance,
                    wallet.AvailableBalance,
                    resetAt),
                cancellationToken);

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
