using Microsoft.AspNetCore.Identity;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Domain.Users;

namespace TradingSimulator.Infrastructure.Auth;

internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationIdentityUser> _passwordHasher = new();

    public PasswordHash Hash(Password password) =>
        PasswordHash.Create(_passwordHasher.HashPassword(new ApplicationIdentityUser(), password.Value));
}
