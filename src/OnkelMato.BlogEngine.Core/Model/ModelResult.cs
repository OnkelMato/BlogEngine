namespace OnkelMato.BlogEngine.Core.Model;

public class ModelResult<T> where T : class
{
    public bool IsSuccess => Value is not null;
    public bool IsFailure => ErrorMessage is not null;

    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }

    private ModelResult() { }

    public static ModelResult<T> Success(T value) => new() { Value = value };
    public static ModelResult<T> Failure(string errorMessage) => new() { ErrorMessage = errorMessage };
}