using TradingSimulator.Application.Common;

namespace TradingSimulator.Application.Portfolios.Commands;

internal static class PortfolioResetErrors
{
    public static readonly Error WalletNotFound = Error.NotFound(
        "WALLET_NOT_FOUND",
        "Wallet was not found for the current user.");

    public static readonly Error ResetInProgress = Error.Conflict(
        "RESET_IN_PROGRESS",
        "A portfolio reset is already in progress for your account.");

    public static Error CooldownActive(DateTimeOffset nextEligibleAt) =>
        Error.Validation(
            "RESET_COOLDOWN_ACTIVE",
            "Portfolio reset is on cooldown.",
            new Dictionary<string, object?>
            {
                ["nextEligibleAt"] = nextEligibleAt.ToString("O"),
            });
}
