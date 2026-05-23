using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Messaging;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Common;
using TradingSimulator.Contracts.Users;

namespace TradingSimulator.Application.Users.Queries;

public sealed class GetMyWalletQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IWalletReadRepository walletReadRepository) : QueryHandler<GetMyWalletQuery, WalletResponse>
{
    public override async Task<Result<WalletResponse>> Handle(
        GetMyWalletQuery query,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId;
        if (userId is null)
        {
            return Error.Unauthorized("UNAUTHORIZED", "Authentication is required.");
        }

        var wallet = await walletReadRepository.GetByUserIdAsync(userId.Value, cancellationToken);
        if (wallet is null)
        {
            return Error.NotFound("WALLET_NOT_FOUND", "Wallet was not found for the current user.");
        }

        return new WalletResponse(
            wallet.UserId,
            wallet.Username,
            wallet.Currency,
            wallet.TotalBalance,
            wallet.ReservedBalance,
            wallet.TotalBalance - wallet.ReservedBalance);
    }
}
