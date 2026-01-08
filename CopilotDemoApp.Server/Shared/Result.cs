using System;

namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Represents the result of an operation, supporting Railway Oriented Programming.
/// </summary>
public class Result<T>
{
	public bool IsSuccess { get; }
	public T? Value { get; }
	public Error? Error { get; }

	private Result(T value)
	{
		IsSuccess = true;
		Value = value;
		Error = null;
	}

	private Result(Error error)
	{
		IsSuccess = false;
		Value = default;
		Error = error;
	}

	public static Result<T> Success(T value) => new(value);
	public static Result<T> Failure(Error error) => new(error);

	/// <summary>
	/// Chains the next operation if successful, propagates error otherwise.
	/// </summary>
	public Result<U> Bind<U>(Func<T, Result<U>> func)
	{
		if (IsSuccess && Value is not null)
			return func(Value);
		return Result<U>.Failure(Error!);
	}

	/// <summary>
	/// Maps the value if successful, propagates error otherwise.
	/// </summary>
	public Result<U> Map<U>(Func<T, U> func)
	{
		if (IsSuccess && Value is not null)
			return Result<U>.Success(func(Value));
		return Result<U>.Failure(Error!);
	}

	/// <summary>
	/// Converts to a void result (Unit) if successful.
	/// </summary>
	public Result<Unit> ToUnit()
	{
		return IsSuccess ? Result<Unit>.Success(Unit.Value) : Result<Unit>.Failure(Error!);
	}
	/// <summary>
	/// Pattern matching for success and failure cases.
	/// </summary>
	public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure)
	{
		if (IsSuccess && Value is not null)
			return onSuccess(Value);
		return onFailure(Error!);
	}
}
