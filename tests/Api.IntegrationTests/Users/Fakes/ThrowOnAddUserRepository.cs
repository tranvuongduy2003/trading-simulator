using Microsoft.EntityFrameworkCore;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Domain.Portfolios;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence;

namespace TradingSimulator.Api.IntegrationTests.Users.Fakes;

internal sealed class ThrowOnAddUserRepository(ApplicationDatabaseContext databaseContext) : IUserRepository
{
    public Task AddAsync(User user, Portfolio portfolio, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("Simulated persistence failure.");

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        databaseContext.Users.AsNoTracking().AnyAsync(user => user.Username == username, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        databaseContext.Users.AsNoTracking().AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
}
