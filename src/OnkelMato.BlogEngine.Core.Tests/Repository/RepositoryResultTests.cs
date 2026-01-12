using FluentAssertions;
using NUnit.Framework;
using OnkelMato.BlogEngine.Core.Repository;

namespace OnkelMato.BlogEngine.Core.Tests.Repository;

[TestFixture]
public class RepositoryResultTests
{
    [Test]
    public void RepositoryResult_Success_CreatesSuccessfulResult()
    {
        var result = RepositoryResult<object>.Success(new object());
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }
    [Test]
    public void RepositoryResult_Failure_CreatesFailedResult()
    {
        var errorMessage = "An error occurred.";
        var result = RepositoryResult<object>.Failure(errorMessage);
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Be(errorMessage);
    }
}