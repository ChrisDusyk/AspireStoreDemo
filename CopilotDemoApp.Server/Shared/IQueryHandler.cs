namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Handles a query and returns a result asynchronously.
/// </summary>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
	Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
