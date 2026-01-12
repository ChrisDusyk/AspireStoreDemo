namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Handles a command and returns a result asynchronously.
/// </summary>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
	Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
