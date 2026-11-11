using Vitalita.API.Shared.Application.Patterns;

namespace Vitalita.API.Tests.Shared.Application.Patterns;

public class ResultTests
{
    private enum SampleError
    {
        Duplicated
    }

    private static Result<int, SampleError> Success(int value) => new Result<int, SampleError>.Success(value);

    private static Result<int, SampleError> Failure(SampleError error) => new Result<int, SampleError>.Failure(error);

    [Fact]
    public void Success_IsSuccessAndNotFailure()
    {
        var result = Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailureAndNotSuccess()
    {
        var result = Failure(SampleError.Duplicated);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Map_OnSuccess_TransformsTheValue()
    {
        var mapped = Success(21).Map(value => value * 2);

        var success = Assert.IsType<Result<int, SampleError>.Success>(mapped);
        Assert.Equal(42, success.Value);
    }

    [Fact]
    public void Map_OnFailure_KeepsTheErrorAndDoesNotCallTheFunction()
    {
        var called = false;

        var mapped = Failure(SampleError.Duplicated).Map(value =>
        {
            called = true;
            return value.ToString();
        });

        var failure = Assert.IsType<Result<string, SampleError>.Failure>(mapped);
        Assert.Equal(SampleError.Duplicated, failure.Error);
        Assert.False(called);
    }

    [Fact]
    public void Fold_UsesTheBranchThatMatchesTheResult()
    {
        Assert.Equal("value:7", Success(7).Fold(v => $"value:{v}", e => $"error:{e}"));
        Assert.Equal("error:Duplicated", Failure(SampleError.Duplicated).Fold(v => $"value:{v}", e => $"error:{e}"));
    }

    [Fact]
    public void Match_RunsOnlyTheMatchingAction()
    {
        int? seenValue = null;
        SampleError? seenError = null;

        Success(5).Match(v => seenValue = v, e => seenError = e);

        Assert.Equal(5, seenValue);
        Assert.Null(seenError);

        seenValue = null;
        Failure(SampleError.Duplicated).Match(v => seenValue = v, e => seenError = e);

        Assert.Null(seenValue);
        Assert.Equal(SampleError.Duplicated, seenError);
    }
}
