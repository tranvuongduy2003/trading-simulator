using TradingSimulator.Domain.Portfolios;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task AddAsync(User user, Portfolio portfolio, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
}
