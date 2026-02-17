using Azure.Messaging.ServiceBus;
using CopilotDemoApp.Shared.Messages;
using System.Text.Json;

namespace CopilotDemoApp.Worker;

/// <summary>
/// Background service that processes OrderCreatedEvent messages from the Azure Service Bus queue.
/// Logs order details and handles message processing failures via dead-lettering.
/// </summary>
public sealed class OrderProcessor : BackgroundService
{
	private readonly ILogger<OrderProcessor> _logger;
	private readonly ServiceBusClient _serviceBusClient;
	private ServiceBusProcessor? _processor;

	private const string QueueName = "orders";
	private readonly JsonSerializerOptions _serializerOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public OrderProcessor(
		ILogger<OrderProcessor> logger,
		ServiceBusClient serviceBusClient)
	{
		_logger = logger;
		_serviceBusClient = serviceBusClient;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// Create processor for the orders queue
		var processorOptions = new ServiceBusProcessorOptions
		{
			MaxConcurrentCalls = 1,
			AutoCompleteMessages = false,
			MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
		};

		_processor = _serviceBusClient.CreateProcessor(QueueName, processorOptions);

		// Configure event handlers
		_processor.ProcessMessageAsync += ProcessMessageAsync;
		_processor.ProcessErrorAsync += ProcessErrorAsync;

		_logger.LogInformation("Starting order processor for queue '{QueueName}'", QueueName);

		try
		{
			await _processor.StartProcessingAsync(stoppingToken);

			// Keep the service running until cancellation is requested
			await Task.Delay(Timeout.Infinite, stoppingToken);
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("Order processor is shutting down");
		}
	}

	private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
	{
		var messageId = args.Message.MessageId;
		var enqueuedTime = args.Message.EnqueuedTime;

		try
		{
			// Deserialize the message envelope
			var messageBody = args.Message.Body.ToString();
			var envelope = JsonSerializer.Deserialize<MessageEnvelope<OrderCreatedEvent>>(messageBody, _serializerOptions);

			if (envelope?.Payload is null)
			{
				_logger.LogWarning(
					"Received invalid message. MessageId: {MessageId}, EnqueuedTime: {EnqueuedTime}",
					messageId,
					enqueuedTime);

				// Dead-letter invalid messages
				await args.DeadLetterMessageAsync(args.Message, "InvalidMessage", "Message envelope or payload is null");
				return;
			}

			var orderEvent = envelope.Payload;

			// Log the order details
			_logger.LogInformation(
				"Processing order created event. " +
				"OrderId: {OrderId}, " +
				"CustomerEmail: {CustomerEmail}, " +
				"TotalAmount: {TotalAmount:C}, " +
				"ItemCount: {ItemCount}, " +
				"OrderDate: {OrderDate}, " +
				"MessageId: {MessageId}, " +
				"CorrelationId: {CorrelationId}",
				orderEvent.OrderId,
				orderEvent.UserEmail,
				orderEvent.TotalAmount,
				orderEvent.LineItems.Count,
				orderEvent.OrderDate,
				envelope.MessageId,
				envelope.CorrelationId);

			// Complete the message
			await args.CompleteMessageAsync(args.Message);

			_logger.LogInformation(
				"Successfully processed order event. OrderId: {OrderId}, MessageId: {MessageId}",
				orderEvent.OrderId,
				messageId);
		}
		catch (JsonException ex)
		{
			_logger.LogError(
				ex,
				"Failed to deserialize message. MessageId: {MessageId}, EnqueuedTime: {EnqueuedTime}",
				messageId,
				enqueuedTime);

			// Dead-letter messages that can't be deserialized
			await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError(
				ex,
				"Unexpected error processing message. MessageId: {MessageId}, EnqueuedTime: {EnqueuedTime}",
				messageId,
				enqueuedTime);

			// Dead-letter messages that fail processing
			await args.DeadLetterMessageAsync(args.Message, "ProcessingError", ex.Message);
		}
	}

	private Task ProcessErrorAsync(ProcessErrorEventArgs args)
	{
		_logger.LogError(
			args.Exception,
			"Error in service bus processor. Source: {ErrorSource}, EntityPath: {EntityPath}",
			args.ErrorSource,
			args.EntityPath);

		return Task.CompletedTask;
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping order processor");

		if (_processor != null)
		{
			await _processor.StopProcessingAsync(cancellationToken);
			await _processor.DisposeAsync();
		}

		await base.StopAsync(cancellationToken);
	}
}
