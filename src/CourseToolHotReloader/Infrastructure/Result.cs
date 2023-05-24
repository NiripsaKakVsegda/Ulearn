using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace CourseToolHotReloader.Infrastructure;

public class None
{
	private None()
	{
	}
}

public struct Result<T>
{
	public Result(string error, Exception? e = null)
	{
		Error = error;
		Exception = e;
	}

	public Result(T value)
	{
		Value = value;
	}

	public static implicit operator Result<T>(T v)
	{
		return Result.Ok(v);
	}

	internal T? Value { get; }
	public string? Error { get; }
	public Exception? Exception { get; }

	public T GetValueOrThrow()
	{
		if (IsSuccess)
			return Value;
		throw new InvalidOperationException($"No value. Only Error {Error}");
	}

	public T? GetValueOrDefault(T? defaultValue) =>
		IsSuccess ? Value : defaultValue;

	[MemberNotNullWhen(false, nameof(Error))]
	[MemberNotNullWhen(true, nameof(Value))]
	public bool IsSuccess => Error is null;
}

public static class Result
{
	public static Result<T> AsResult<T>(this T value)
	{
		return Ok(value);
	}

	public static Result<T> Ok<T>(T value)
	{
		return new Result<T>(value);
	}

	public static Result<None> Ok()
	{
		return Ok<None>(null!);
	}

	public static Result<T> Fail<T>(string error, Exception? e = null)
	{
		return new Result<T>(error, e);
	}

	public static Result<T> Of<T>(Func<T> f, Func<Exception, string>? errorBuilder = null)
	{
		try
		{
			return Ok(f());
		}
		catch (Exception e)
		{
			return Fail<T>(errorBuilder is null ? e.Message : errorBuilder(e), e);
		}
	}

	public static Result<None> OfAction(Action f, Func<Exception, string>? errorBuilder = null)
	{
		try
		{
			f();
			return Ok();
		}
		catch (Exception e)
		{
			return Fail<None>(errorBuilder is null ? e.Message : errorBuilder(e), e);
		}
	}

	public static Result<TOutput> Then<TInput, TOutput>(
		this Result<TInput> input,
		Func<TInput, TOutput> continuation)
	{
		return input.Then(inp => Of(() => continuation(inp)));
	}

	public static Result<None> Then<TInput>(
		this Result<TInput> input,
		Action<TInput> continuation)
	{
		return input.Then(inp => OfAction(() => continuation(inp)));
	}

	public static Result<TOutput> Then<TInput, TOutput>(
		this Result<TInput> input,
		Func<TInput, Result<TOutput>> continuation
	)
	{
		return input.IsSuccess
			? continuation(input.Value)
			: Fail<TOutput>(input.Error, input.Exception);
	}

	public static Result<TInput> OnFail<TInput>(
		this Result<TInput> input,
		Action<string, Exception?> handleError
	)
	{
		if (!input.IsSuccess)
			handleError(input.Error, input.Exception);
		return input;
	}

	public static Result<TInput> ReplaceError<TInput>(
		this Result<TInput> input,
		Func<string, string> replaceError)
	{
		return input.IsSuccess
			? input
			: Fail<TInput>(replaceError(input.Error), input.Exception);
	}

	public static Result<TInput> RefineError<TInput>(
		this Result<TInput> input,
		string errorMessage
	)
	{
		return input.ReplaceError(err => errorMessage + ". " + err);
	}

	public static Result<TInput> ReplaceException<TInput>(
		this Result<TInput> input,
		Func<Exception?, Exception?> replaceException)
	{
		return input.IsSuccess
			? input
			: Fail<TInput>(input.Error, replaceException(input.Exception));
	}
}

public static class ResultAsync
{
	public static async Task<Result<T>> Of<T>(Func<Task<T>> f, Func<Exception, string>? errorBuilder = null)
	{
		try
		{
			return Result.Ok(await f());
		}
		catch (Exception e)
		{
			return Result.Fail<T>(errorBuilder is null ? e.Message : errorBuilder(e), e);
		}
	}

	public static async Task<Result<None>> OfAction(Func<Task> f, Func<Exception, string>? errorBuilder = null)
	{
		try
		{
			await f();
			return Result.Ok();
		}
		catch (Exception e)
		{
			return Result.Fail<None>(errorBuilder is null ? e.Message : errorBuilder(e), e);
		}
	}

	public static Task<Result<TOutput>> Then<TInput, TOutput>(
		this Task<Result<TInput>> input,
		Func<TInput, TOutput> continuation
	)
	{
		return input.Then(inp => Result.Of(() => continuation(inp)));
	}

	public static Task<Result<None>> Then<TInput>(
		this Task<Result<TInput>> input,
		Action<TInput> continuation
	)
	{
		return input.Then(inp => Result.OfAction(() => continuation(inp)));
	}

	public static Task<Result<None>> Then<TInput>(
		this Task<Result<TInput>> input,
		Func<TInput, Task> continuation
	)
	{
		return input.Then(inp => OfAction(() => continuation(inp)));
	}

	public static async Task<Result<TOutput>> Then<TInput, TOutput>(
		this Task<Result<TInput>> inputTask,
		Func<TInput, Result<TOutput>> continuation
	)
	{
		var input = await inputTask;
		return input.IsSuccess
			? continuation(input.Value)
			: Result.Fail<TOutput>(input.Error, input.Exception);
	}

	public static async Task<Result<TOutput>> Then<TInput, TOutput>(
		this Result<TInput> input,
		Func<TInput, Task<Result<TOutput>>> continuation
	)
	{
		return input.IsSuccess
			? await continuation(input.Value)
			: Result.Fail<TOutput>(input.Error, input.Exception);
	}

	public static async Task<Result<TOutput>> Then<TInput, TOutput>(
		this Task<Result<TInput>> inputTask,
		Func<TInput, Task<Result<TOutput>>> continuation
	)
	{
		var input = await inputTask;
		return input.IsSuccess
			? await continuation(input.Value)
			: Result.Fail<TOutput>(input.Error, input.Exception);
	}

	public static async Task<Result<TInput>> OnFail<TInput>(
		this Task<Result<TInput>> inputTask,
		Action<string, Exception?> handleError
	)
	{
		var input = await inputTask;
		if (!input.IsSuccess)
			handleError(input.Error, input.Exception);
		return input;
	}

	public static async Task<Result<TInput>> ReplaceError<TInput>(
		this Task<Result<TInput>> inputTask,
		Func<string, string> replaceError)
	{
		var input = await inputTask;
		return input.IsSuccess
			? input
			: Result.Fail<TInput>(replaceError(input.Error), input.Exception);
	}

	public static Task<Result<TInput>> RefineError<TInput>(
		this Task<Result<TInput>> inputTask,
		string errorMessage
	)
	{
		return inputTask.ReplaceError(err => errorMessage + ". " + err);
	}

	public static async Task<Result<TInput>> ReplaceException<TInput>(
		this Task<Result<TInput>> inputTask,
		Func<Exception?, Exception?> replaceException)
	{
		var input = await inputTask;
		return input.IsSuccess
			? input
			: Result.Fail<TInput>(input.Error, replaceException(input.Exception));
	}
}