namespace OnkelMato.BlogEngine.Core.Repository;

public class RepositoryResult<T> where T : class
{
    public bool IsSuccess => Value is not null;
    public bool IsFailure => ErrorMessage is not null;

    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }

    private RepositoryResult() { }

    public static RepositoryResult <T> Success(T value) => new() { Value = value };
    public static RepositoryResult<T> Failure(string errorMessage) => new() { ErrorMessage = errorMessage };
}