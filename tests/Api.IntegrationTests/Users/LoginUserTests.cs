using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TradingSimulator.Api.IntegrationTests.Integration;
using TradingSimulator.Application.Abstractions.Auth;
using TradingSimulator.Application.Abstractions.Persistence;
using TradingSimulator.Application.Users;
using TradingSimulator.Application.Users.Commands;
using TradingSimulator.Contracts.Portfolio;
using TradingSimulator.Contracts.Users;
using TradingSimulator.Domain.Users;
using TradingSimulator.Infrastructure.Persistence;
using TradingSimulator.Testing.Common.Fixtures;

namespace TradingSimulator.Api.IntegrationTests.Users;

[Collection(IntegrationTestCollection.Name)]
public sealed class LoginUserTests(IntegrationTestFixture fixture)
{
    private readonly HttpClient _client = fixture.Factory.CreateClient(
        new WebApplicationFactoryClientOptions { HandleCookies = true });

    private HttpClient CreateClient() =>
        fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });

    [Fact]
    public async Task LoginUser_Returns200_AndWalletShowsRegisteredUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerClient = CreateClient();
        using var loginClient = CreateClient();

        using var registerResponse = await registerClient.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var unauthenticatedWalletResponse = await loginClient.GetAsync("/api/wallet");
        unauthenticatedWalletResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        loginResponse.Headers.Should().ContainKey("Set-Cookie");

        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginUserResponse>();
        loginBody.Should().NotBeNull();
        loginBody!.UserId.Should().Be(registration!.UserId);
        loginBody.Username.Should().Be(registration.Username);

        using var walletResponse = await loginClient.GetAsync("/api/wallet");

        walletResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var wallet = await walletResponse.Content.ReadFromJsonAsync<WalletResponse>();
        wallet.Should().NotBeNull();
        wallet!.UserId.Should().Be(registration.UserId);
        wallet.Username.Should().Be(registration.Username);
        wallet.Currency.Should().Be("USD");
        wallet.AvailableBalance.Should().Be(100_000m);
        wallet.TotalBalance.Should().Be(100_000m);
        wallet.ReservedBalance.Should().Be(0m);
    }

    [Fact]
    public async Task LoginUser_AfterRegister_PortfolioReturnsHoldings()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerClient = CreateClient();
        using var loginClient = CreateClient();

        using var registerResponse = await registerClient.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var registration = await registerResponse.Content.ReadFromJsonAsync<UserRegistrationResponse>();
        registration.Should().NotBeNull();

        using var loginResponse = await loginClient.PostAsJsonAsync(
            "/api/auth/login",
            new LoginUserRequest(email, password));

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var portfolioResponse = await loginClient.GetAsync("/api/portfolio");

        portfolioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var portfolio = await portfolioResponse.Content.ReadFromJsonAsync<PortfolioResponse>();
        portfolio.Should().NotBeNull();
        portfolio!.UserId.Should().Be(registration!.UserId);
        portfolio.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task LoginUser_EndpointExists_Returns401_InvalidCredentials()
    {
        var request = new LoginUserRequest("trader@example.com", "SecurePass1!");

        using var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginUser_MalformedJson_Returns400_INVALID_REQUEST()
    {
        using var response = await _client.PostAsync(
            "/api/auth/login",
            RegisterUserTestHelpers.JsonContent("{"));

        await RegisterUserTestHelpers.AssertInvalidRequestAsync(response);
    }

    [Fact]
    public async Task PasswordHasher_Verify_ReturnsTrue_ForRegisteredPassword()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";
        const string password = "SecurePass1!";

        using var registerResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, password));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var normalizedEmail = EmailAddress.Create(email).Value;
        var user = await userRepository.GetByEmailAsync(normalizedEmail);

        user.Should().NotBeNull();
        passwordHasher.Verify(Password.Create(password), user!.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task UserRepository_GetByEmailAsync_ReturnsNull_WhenUnknown()
    {
        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.GetByEmailAsync($"missing_{Guid.NewGuid():N}@example.com");

        user.Should().BeNull();
    }

    [Fact]
    public async Task UserRepository_GetByEmailAsync_ReturnsUser_WithPasswordHash_WhenRegistered()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";

        using var registerResponse = await _client.PostAsJsonAsync(
            "/api/users",
            new RegisterUserRequest($"trader_{suffix}", email, "SecurePass1!"));

        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var normalizedEmail = EmailAddress.Create(email).Value;
        var user = await userRepository.GetByEmailAsync(normalizedEmail);

        user.Should().NotBeNull();
        user!.PasswordHash.Value.Should().NotBeNullOrWhiteSpace();
        user.Username.Value.Should().Be($"trader_{suffix}");
    }

    [Fact]
    public async Task LoginUserCommand_Succeeds_ForRegisteredUser()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";
        const string password = "SecurePass1!";

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var registerResult = await sender.Send(
            new RegisterUserCommand($"trader_{suffix}", email, password));

        registerResult.IsSuccess.Should().BeTrue();

        var loginResult = await sender.Send(new LoginUserCommand(email, password));

        loginResult.IsSuccess.Should().BeTrue();
        loginResult.Value!.UserId.Should().Be(registerResult.Value!.UserId);
        loginResult.Value.Username.Should().Be($"trader_{suffix}");
        loginResult.Value.SessionId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoginUserCommand_WrongPassword_ReturnsInvalidCredentials()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var registerResult = await sender.Send(
            new RegisterUserCommand($"trader_{suffix}", email, "SecurePass1!"));

        registerResult.IsSuccess.Should().BeTrue();

        var loginResult = await sender.Send(new LoginUserCommand(email, "WrongPass1!"));

        loginResult.IsFailure.Should().BeTrue();
        loginResult.Error!.Code.Should().Be(LoginErrors.InvalidCredentialsCode);
    }

    [Fact]
    public async Task LoginUserCommand_DoesNotModify_WalletPortfolioOrHoldings()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var email = $"login_{suffix}@example.com";

        await using var scope = fixture.Factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var databaseContext = scope.ServiceProvider.GetRequiredService<ApplicationDatabaseContext>();

        var registerResult = await sender.Send(
            new RegisterUserCommand($"trader_{suffix}", email, "SecurePass1!"));

        registerResult.IsSuccess.Should().BeTrue();

        var walletsAfterRegister = await databaseContext.Wallets.CountAsync();
        var portfoliosAfterRegister = await databaseContext.Portfolios.CountAsync();
        var holdingsAfterRegister = await databaseContext.Holdings.CountAsync();

        var loginResult = await sender.Send(new LoginUserCommand(email, "SecurePass1!"));

        loginResult.IsSuccess.Should().BeTrue();

        (await databaseContext.Wallets.CountAsync()).Should().Be(walletsAfterRegister);
        (await databaseContext.Portfolios.CountAsync()).Should().Be(portfoliosAfterRegister);
        (await databaseContext.Holdings.CountAsync()).Should().Be(holdingsAfterRegister);
    }
}
