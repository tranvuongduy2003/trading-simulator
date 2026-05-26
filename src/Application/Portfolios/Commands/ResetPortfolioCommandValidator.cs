using FluentValidation;

namespace TradingSimulator.Application.Portfolios.Commands;

public sealed class ResetPortfolioCommandValidator : AbstractValidator<ResetPortfolioCommand>
{
    public ResetPortfolioCommandValidator()
    {
    }
}
