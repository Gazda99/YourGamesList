namespace YourGamesList.Common.UnitTests;

public class ResultsTests
{
    [Test]
    public void CombinedResult_Success_CreatesValidCombinedResult()
    {
        //ARRANGE
        var value = new TestValueClass();

        //ACT
        var result = CombinedResult<TestValueClass, TestErrorClass>.Success(value);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(value));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void CombinedResult_Failure_CreatesValidCombinedResult()
    {
        //ARRANGE
        var error = new TestErrorClass();

        //ACT
        var result = CombinedResult<TestValueClass, TestErrorClass>.Failure(error);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Value, Is.Null);
        Assert.That(result.Error, Is.EqualTo(error));
    }


    [Test]
    public void ErrorResult_Clear_CreatesValidErrorResult()
    {
        //ACT
        var result = ErrorResult<TestErrorClass>.Clear();

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void ErrorResult_Failure_CreatesValidErrorResult()
    {
        //ARRANGE
        var error = new TestErrorClass();

        //ACT
        var result = ErrorResult<TestErrorClass>.Failure(error);

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo(error));
    }

    [Test]
    public void ValueResult_Success_CreatesValidErrorResult()
    {
        //ARRANGE
        var value = new TestValueClass();

        //ACT
        var result = ValueResult<TestValueClass>.Success(value);

        //ASSERT
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(value));
    }

    [Test]
    public void ValueResult_Failure_CreatesValidErrorResult()
    {
        //ACT
        var result = ValueResult<TestValueClass>.Failure();

        //ASSERT
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    public class TestValueClass
    {
    }

    public class TestErrorClass
    {
    }
}