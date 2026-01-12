using FluentAssertions;
using NUnit.Framework;
using OnkelMato.BlogEngine.Core.Model;

namespace OnkelMato.BlogEngine.Core.Tests.Repository;

[TestFixture]
public class ModelResultTests
{
    #region Success Tests

    [Test]
    public void Success_WithValidValue_CreatesSuccessfulResult()
    {
        // Arrange
        var testValue = new TestModel { Name = "Test" };

        // Act
        var result = ModelResult<TestModel>.Success(testValue);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().NotBeNull();
        result.Value.Should().Be(testValue);
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Success_WithNullValue_CreatesResultWithNullValue()
    {
        // Act
        var result = ModelResult<TestModel>.Success(null!);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse(); // IsSuccess checks if Value is not null
        result.IsFailure.Should().BeFalse();
        result.Value.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Success_WithString_CreatesSuccessfulResult()
    {
        // Arrange
        var testString = "Test String";

        // Act
        var result = ModelResult<string>.Success(testString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(testString);
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Success_WithComplexObject_CreatesSuccessfulResult()
    {
        // Arrange
        var complexObject = new ComplexTestModel
        {
            Id = 1,
            Name = "Complex",
            NestedModel = new TestModel { Name = "Nested" }
        };

        // Act
        var result = ModelResult<ComplexTestModel>.Success(complexObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(complexObject);
        result.Value!.NestedModel.Should().NotBeNull();
        result.Value.NestedModel!.Name.Should().Be("Nested");
    }

    #endregion

    #region Failure Tests

    [Test]
    public void Failure_WithErrorMessage_CreatesFailedResult()
    {
        // Arrange
        var errorMessage = "An error occurred";

        // Act
        var result = ModelResult<TestModel>.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);
    }

    [Test]
    public void Failure_WithEmptyErrorMessage_CreatesFailedResultWithEmptyMessage()
    {
        // Arrange
        var errorMessage = string.Empty;

        // Act
        var result = ModelResult<TestModel>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Test]
    public void Failure_WithNullErrorMessage_CreatesFailedResultWithNullMessage()
    {
        // Act
        var result = ModelResult<TestModel>.Failure(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeFalse(); // IsFailure checks if ErrorMessage is not null
        result.Value.Should().BeNull();
        result.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Failure_WithLongErrorMessage_CreatesFailedResult()
    {
        // Arrange
        var errorMessage = new string('x', 1000);

        // Act
        var result = ModelResult<TestModel>.Failure(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().HaveLength(1000);
    }

    #endregion

    #region IsSuccess and IsFailure Tests

    [Test]
    public void IsSuccess_WithValidValue_ReturnsTrue()
    {
        // Arrange
        var result = ModelResult<TestModel>.Success(new TestModel { Name = "Test" });

        // Act & Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void IsSuccess_WithNullValue_ReturnsFalse()
    {
        // Arrange
        var result = ModelResult<TestModel>.Success(null!);

        // Act & Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public void IsFailure_WithErrorMessage_ReturnsTrue()
    {
        // Arrange
        var result = ModelResult<TestModel>.Failure("Error");

        // Act & Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void IsFailure_WithNullErrorMessage_ReturnsFalse()
    {
        // Arrange
        var result = ModelResult<TestModel>.Failure(null!);

        // Act & Assert
        result.IsFailure.Should().BeFalse();
    }

    [Test]
    public void IsSuccess_AndIsFailure_AreMutuallyExclusive()
    {
        // Arrange
        var successResult = ModelResult<TestModel>.Success(new TestModel { Name = "Test" });
        var failureResult = ModelResult<TestModel>.Failure("Error");

        // Act & Assert
        successResult.IsSuccess.Should().BeTrue();
        successResult.IsFailure.Should().BeFalse();

        failureResult.IsSuccess.Should().BeFalse();
        failureResult.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Test]
    public void ModelResult_BothValueAndErrorMessageNull_IsNeitherSuccessNorFailure()
    {
        // This shouldn't happen in normal usage but tests the logic
        // Arrange & Act
        var result = ModelResult<TestModel>.Success(null!);
        var result2 = ModelResult<TestModel>.Failure(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeFalse();

        result2.IsSuccess.Should().BeFalse();
        result2.IsFailure.Should().BeFalse();
    }

    [Test]
    public void Success_CalledMultipleTimes_CreatesIndependentResults()
    {
        // Arrange
        var value1 = new TestModel { Name = "First" };
        var value2 = new TestModel { Name = "Second" };

        // Act
        var result1 = ModelResult<TestModel>.Success(value1);
        var result2 = ModelResult<TestModel>.Success(value2);

        // Assert
        result1.Value.Should().NotBe(result2.Value);
        result1.Value!.Name.Should().Be("First");
        result2.Value!.Name.Should().Be("Second");
    }

    [Test]
    public void Failure_CalledMultipleTimes_CreatesIndependentResults()
    {
        // Arrange
        var error1 = "Error 1";
        var error2 = "Error 2";

        // Act
        var result1 = ModelResult<TestModel>.Failure(error1);
        var result2 = ModelResult<TestModel>.Failure(error2);

        // Assert
        result1.ErrorMessage.Should().NotBe(result2.ErrorMessage);
        result1.ErrorMessage.Should().Be("Error 1");
        result2.ErrorMessage.Should().Be("Error 2");
    }

    #endregion

    #region Type-specific Tests

    [Test]
    public void ModelResult_WithDifferentTypes_WorksCorrectly()
    {
        // Arrange & Act
        var stringResult = ModelResult<string>.Success("Test");
        var intResult = ModelResult<TestModel>.Success(new TestModel { Name = "Test" });

        // Assert
        stringResult.IsSuccess.Should().BeTrue();
        stringResult.Value.Should().Be("Test");

        intResult.IsSuccess.Should().BeTrue();
        intResult.Value!.Name.Should().Be("Test");
    }

    [Test]
    public void Success_WithCollection_CreatesSuccessfulResult()
    {
        // Arrange
        var list = new List<TestModel>
        {
            new TestModel { Name = "Item1" },
            new TestModel { Name = "Item2" }
        };

        // Act
        var result = ModelResult<List<TestModel>>.Success(list);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Name.Should().Be("Item1");
    }

    #endregion

    #region Helper Classes

    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    private class ComplexTestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TestModel? NestedModel { get; set; }
    }

    #endregion
}