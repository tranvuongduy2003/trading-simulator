using TradingSimulator.Domain.Users;

namespace TradingSimulator.Application.Abstractions.Auth;

public interface IPasswordHasher
{
    PasswordHash Hash(Password password);
}
