using CopilotDemoApp.Server.Shared.Messages;

namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Publishes order-related messages to the message queue.
/// </summary>
public interface IOrderMessagePublisher
{
	/// <summary>
	/// Publishes an OrderCreatedEvent to the orders queue.
	/// </summary>
	/// <param name="orderEvent">The order created event to publish</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>Result indicating success or failure of the publish operation</returns>
	Task<Result<Unit>> PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default);
}
