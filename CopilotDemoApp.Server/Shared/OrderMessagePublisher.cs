using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CopilotDemoApp.Shared.Messages;

namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Implementation of IOrderMessagePublisher that publishes messages to Azure Service Bus.
/// </summary>
public sealed class OrderMessagePublisher(ServiceBusClient serviceBusClient, ILogger<OrderMessagePublisher> logger) : IOrderMessagePublisher
{
	private const string OrdersQueueName = "orders";
	private readonly JsonSerializerOptions _serializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public async Task<Result<Unit>> PublishOrderCreatedAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken = default)
	{
		try
		{
			// Wrap event in message envelope
			var envelope = new MessageEnvelope<OrderCreatedEvent>(
				MessageId: Guid.NewGuid(),
				Timestamp: DateTimeOffset.UtcNow,
				MessageType: nameof(OrderCreatedEvent),
				Payload: orderEvent,
				CorrelationId: orderEvent.OrderId
			);

			// Serialize envelope to JSON

			var messageBody = JsonSerializer.Serialize(envelope, _serializerOptions);

			// Create Service Bus message
			var serviceBusMessage = new ServiceBusMessage(messageBody)
			{
				ContentType = "application/json",
				MessageId = envelope.MessageId.ToString(),
				CorrelationId = envelope.CorrelationId?.ToString()
			};

			// Send message to queue
			var sender = serviceBusClient.CreateSender(OrdersQueueName);
			await using (sender.ConfigureAwait(false))
			{
				await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
			}

			logger.LogInformation(
				"Successfully published OrderCreatedEvent for Order {OrderId} with MessageId {MessageId}",
				orderEvent.OrderId,
				envelope.MessageId
			);

			return Result<Unit>.Success(Unit.Value);
		}
		catch (Exception ex)
		{
			logger.LogError(
				ex,
				"Failed to publish OrderCreatedEvent for Order {OrderId}",
				orderEvent.OrderId
			);

			return Result<Unit>.Failure(new Error(
				ErrorCodes.MessagePublishingFailed,
				$"Failed to publish order created message for order {orderEvent.OrderId}",
				ex
			));
		}
	}
}
