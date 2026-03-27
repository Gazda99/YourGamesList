using System;
using System.Diagnostics.CodeAnalysis;

namespace YourGamesList.Common;

public class CombinedResult<TValue, TError>
{
    public TValue? Value { get; init; }
    public TError? Error { get; init; }

    private CombinedResult(TValue value)
    {
        IsSuccess = true;
        Value = value;
        Error = default!;
    }

    private CombinedResult(TError error)
    {
        IsSuccess = false;
        Value = default!;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; init; }

    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public static CombinedResult<TValue, TError> Success(TValue value) => new CombinedResult<TValue, TError>(value);
    public static CombinedResult<TValue, TError> Failure(TError error) => new CombinedResult<TValue, TError>(error);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure) =>
        IsSuccess ? success(Value!) : failure(Error!);
}

public class ErrorResult<TError>
{
    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure { get; }

    [MemberNotNullWhen(false, nameof(Error))]

    public bool IsSuccess => !IsFailure;

    public TError? Error { get; }

    private ErrorResult(TError error)
    {
        IsFailure = true;
        Error = error;
    }

    private ErrorResult()
    {
        IsFailure = false;
        Error = default;
    }

    public static ErrorResult<TError> Clear() => new ErrorResult<TError>();
    public static ErrorResult<TError> Failure(TError error) => new ErrorResult<TError>(error);
}

public class ValueResult<TValue>
{
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsFailure => !IsSuccess;

    public TValue? Value { get; }

    private ValueResult(TValue value)
    {
        IsSuccess = true;
        Value = value;
    }

    private ValueResult()
    {
        IsSuccess = false;
        Value = default;
    }

    public static ValueResult<TValue> Success(TValue value) => new ValueResult<TValue>(value);

    public static ValueResult<TValue> Failure() => new ValueResult<TValue>();
}