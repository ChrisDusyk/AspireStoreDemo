using System.Reflection;
using CopilotDemoApp.Server.Shared;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class CqrsInfrastructureTests
{
	private class TestQuery : IQuery<string> { }
	private class TestCommand : ICommand<int> { }

	private class TestQueryHandler : IQueryHandler<TestQuery, string>
	{
		public Task<Result<string>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default) =>
			Task.FromResult(Result<string>.Success("query-result"));
	}

	private class TestCommandHandler : ICommandHandler<TestCommand, int>
	{
		public Task<Result<int>> HandleAsync(TestCommand command, CancellationToken cancellationToken = default) =>
			Task.FromResult(Result<int>.Success(123));
	}

	[Fact]
	public void AddCqrsHandlers_RegistersHandlersInDI()
	{
		var services = new ServiceCollection();
		services.AddCqrsHandlers(Assembly.GetExecutingAssembly());
		var provider = services.BuildServiceProvider();

		var queryHandler = provider.GetService<IQueryHandler<TestQuery, string>>();
		var commandHandler = provider.GetService<ICommandHandler<TestCommand, int>>();

		Assert.NotNull(queryHandler);
		Assert.NotNull(commandHandler);
	}

	[Fact]
	public async Task QueryHandler_HandleAsync_ReturnsResult()
	{
		var handler = new TestQueryHandler();
		var result = await handler.HandleAsync(new TestQuery(), TestContext.Current.CancellationToken);
		Assert.True(result.IsSuccess);
		Assert.Equal("query-result", result.Value);
	}

	[Fact]
	public async Task CommandHandler_HandleAsync_ReturnsResult()
	{
		var handler = new TestCommandHandler();
		var result = await handler.HandleAsync(new TestCommand(), TestContext.Current.CancellationToken);
		Assert.True(result.IsSuccess);
		Assert.Equal(123, result.Value);
	}
}
