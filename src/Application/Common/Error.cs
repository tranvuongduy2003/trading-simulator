namespace TradingSimulator.Application.Common;

public sealed record Error
{
    public const string ValidationFailedCode = "VALIDATION_FAILED";

    private Error(
        string code,
        string message,
        ErrorType type,
        IReadOnlyDictionary<string, string[]>? validationErrors)
    {
        Code = code;
        Message = message;
        Type = type;
        ValidationErrors = validationErrors;
    }

    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    public static Error Validation(
        string message,
        IReadOnlyDictionary<string, string[]> validationErrors) =>
        new(ValidationFailedCode, message, ErrorType.Validation, validationErrors);

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation, null);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound, null);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict, null);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized, null);

    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden, null);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure, null);
}
