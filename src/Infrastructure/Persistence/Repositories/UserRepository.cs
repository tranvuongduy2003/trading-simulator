using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Portfolios;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence.Mapping;

namespace TradingSimulator.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(ApplicationDatabaseContext databaseContext) : IUserRepository
{
    public async Task AddAsync(User user, Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        await databaseContext.Users.AddAsync(UserPersistenceMapper.ToUserRecord(user), cancellationToken);
        await databaseContext.Wallets.AddAsync(
            UserPersistenceMapper.ToWalletRecord(user.Wallet, user.UpdatedAt),
            cancellationToken);
        await databaseContext.Portfolios.AddAsync(
            UserPersistenceMapper.ToPortfolioRecord(portfolio),
            cancellationToken);
    }

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        databaseContext.Users.AsNoTracking().AnyAsync(user => user.Username == username, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        databaseContext.Users.AsNoTracking().AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
}
