using CopilotDemoApp.Server.Shared;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class ResultTests
{
	[Fact]
	public void Success_HasValue_AndIsSuccess()
	{
		var result = Result<int>.Success(42);
		Assert.True(result.IsSuccess);
		Assert.Equal(42, result.Value);
		Assert.Null(result.Error);
	}

	[Fact]
	public void Failure_HasError_AndIsNotSuccess()
	{
		var error = new Error("E003", "Failure");
		var result = Result<int>.Failure(error);
		Assert.False(result.IsSuccess);
		Assert.Equal(default(int), result.Value);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void Bind_ChainsOnSuccess()
	{
		var result = Result<int>.Success(2)
			.Bind(x => Result<int>.Success(x * 3));
		Assert.True(result.IsSuccess);
		Assert.Equal(6, result.Value);
	}

	[Fact]
	public void Bind_PropagatesError()
	{
		var error = new Error("E004", "Bind failed");
		var result = Result<int>.Failure(error)
			.Bind(x => Result<int>.Success(x * 3));
		Assert.False(result.IsSuccess);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void Map_TransformsValueOnSuccess()
	{
		var result = Result<int>.Success(5)
			.Map(x => x + 1);
		Assert.True(result.IsSuccess);
		Assert.Equal(6, result.Value);
	}

	[Fact]
	public void Map_PropagatesError()
	{
		var error = new Error("E005", "Map failed");
		var result = Result<int>.Failure(error)
			.Map(x => x + 1);
		Assert.False(result.IsSuccess);
		Assert.Equal(error, result.Error);
	}

	[Fact]
	public void ToUnit_ConvertsSuccessToUnit()
	{
		var result = Result<string>.Success("done").ToUnit();
		Assert.True(result.IsSuccess);
		Assert.Equal(Unit.Value, result.Value);
	}

	[Fact]
	public void ToUnit_PropagatesError()
	{
		var error = new Error("E006", "Unit failed");
		var result = Result<string>.Failure(error).ToUnit();
		Assert.False(result.IsSuccess);
		Assert.Equal(error, result.Error);
	}
}
