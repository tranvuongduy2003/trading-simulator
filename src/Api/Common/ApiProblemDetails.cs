using Microsoft.AspNetCore.Mvc;

namespace TradingSimulator.Api.Common;

public sealed class ApiProblemDetails : ProblemDetails
{
    public string? Code { get; set; }
}
