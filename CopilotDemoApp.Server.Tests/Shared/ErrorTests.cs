using CopilotDemoApp.Server.Shared;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class ErrorTests
{
	[Fact]
	public void Error_CanBeCreated_WithCodeAndMessage()
	{
		var error = new Error("E001", "Something went wrong");
		Assert.Equal("E001", error.Code);
		Assert.Equal("Something went wrong", error.Message);
		Assert.Null(error.Exception);
	}

	[Fact]
	public void Error_CanContainException()
	{
		var ex = new InvalidOperationException("fail");
		var error = new Error("E002", "Domain failure", ex);
		Assert.Equal("E002", error.Code);
		Assert.Equal("Domain failure", error.Message);
		Assert.Equal(ex, error.Exception);
	}
}
