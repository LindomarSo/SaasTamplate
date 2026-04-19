namespace SaasStarter.Application.Common;

public sealed record DomainError(string Code, string Message, int HttpStatus)
{
    public static DomainError Conflict(string message) => new("CONFLICT", message, 409);
    public static DomainError NotFound(string message) => new("NOT_FOUND", message, 404);
    public static DomainError Unauthorized(string message) => new("UNAUTHORIZED", message, 401);
    public static DomainError Forbidden(string message) => new("FORBIDDEN", message, 403);
    public static DomainError Invalid(string message) => new("INVALID", message, 400);
    public static DomainError EmailNotConfirmed(string message) => new("EMAIL_NOT_CONFIRMED", message, 401);
    public static DomainError PlanLimitExceeded(string message) => new("PLAN_LIMIT_EXCEEDED", message, 403);
}

public class Result
{
    public bool IsSuccess { get; }
    public DomainError? Error { get; }

    protected Result(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(DomainError error) => new(false, error);

    public static implicit operator Result(DomainError error) => Failure(error);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(true, null) => Value = value;
    private Result(DomainError error) : base(false, error) { }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(DomainError error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(DomainError error) => Failure(error);
}
