using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Server.Shared.Messages;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class OrderMessagePublisherTests
{
	private readonly ServiceBusClient _mockServiceBusClient;
	private readonly ServiceBusSender _mockSender;
	private readonly ILogger<OrderMessagePublisher> _mockLogger;
	private readonly OrderMessagePublisher _publisher;

	public OrderMessagePublisherTests()
	{
		_mockServiceBusClient = Substitute.For<ServiceBusClient>();
		_mockSender = Substitute.For<ServiceBusSender>();
		_mockLogger = Substitute.For<ILogger<OrderMessagePublisher>>();

		_mockServiceBusClient.CreateSender("orders").Returns(_mockSender);

		_publisher = new OrderMessagePublisher(_mockServiceBusClient, _mockLogger);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_WrapsEventInEnvelope()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();
		ServiceBusMessage? sentMessage = null;

		await _mockSender.SendMessageAsync(
			Arg.Do<ServiceBusMessage>(msg => sentMessage = msg),
			Arg.Any<CancellationToken>()
		);

		// Act
		var result = await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(sentMessage);

		var envelope = JsonSerializer.Deserialize<MessageEnvelope<OrderCreatedEvent>>(
			sentMessage!.Body.ToString(),
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
		);

		Assert.NotNull(envelope);
		Assert.Equal(nameof(OrderCreatedEvent), envelope!.MessageType);
		Assert.Equal(orderEvent.OrderId, envelope.Payload.OrderId);
		Assert.Equal(orderEvent.UserId, envelope.Payload.UserId);
		Assert.Equal(orderEvent.OrderId, envelope.CorrelationId);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_SetsMessageIdAndTimestamp()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();
		ServiceBusMessage? sentMessage = null;

		await _mockSender.SendMessageAsync(
			Arg.Do<ServiceBusMessage>(msg => sentMessage = msg),
			Arg.Any<CancellationToken>()
		);

		// Act
		var result = await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(sentMessage);
		Assert.NotNull(sentMessage!.MessageId);
		Assert.Equal("application/json", sentMessage.ContentType);

		var envelope = JsonSerializer.Deserialize<MessageEnvelope<OrderCreatedEvent>>(
			sentMessage.Body.ToString(),
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
		);

		Assert.NotNull(envelope);
		Assert.NotEqual(Guid.Empty, envelope!.MessageId);
		Assert.True(envelope.Timestamp <= DateTimeOffset.UtcNow);
		Assert.True(envelope.Timestamp >= DateTimeOffset.UtcNow.AddMinutes(-1));
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_ReturnsSuccess_WhenMessageSentSuccessfully()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();

		await _mockSender.SendMessageAsync(
			Arg.Any<ServiceBusMessage>(),
			Arg.Any<CancellationToken>()
		);

		// Act
		var result = await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(Unit.Value, result.Value);
		await _mockSender.Received(1).SendMessageAsync(
			Arg.Any<ServiceBusMessage>(),
			Arg.Any<CancellationToken>()
		);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_ReturnsFailure_WhenServiceBusThrows()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();
		var exception = new ServiceBusException("Connection failed", ServiceBusFailureReason.ServiceCommunicationProblem);

		_mockSender.SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(exception);

		// Act
		var result = await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.MessagePublishingFailed, result.Error!.Code);
		Assert.Contains(orderEvent.OrderId.ToString(), result.Error.Message);
		Assert.Same(exception, result.Error.Exception);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_LogsSuccess()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();

		await _mockSender.SendMessageAsync(
			Arg.Any<ServiceBusMessage>(),
			Arg.Any<CancellationToken>()
		);

		// Act
		await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		_mockLogger.Received(1).Log(
			LogLevel.Information,
			Arg.Any<EventId>(),
			Arg.Is<object>(o => o.ToString()!.Contains("Successfully published")),
			null,
			Arg.Any<Func<object, Exception?, string>>()
		);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_LogsError_WhenPublishingFails()
	{
		// Arrange
		var orderEvent = CreateSampleOrderEvent();
		var exception = new ServiceBusException("Connection failed", ServiceBusFailureReason.ServiceCommunicationProblem);

		_mockSender.SendMessageAsync(Arg.Any<ServiceBusMessage>(), Arg.Any<CancellationToken>())
			.ThrowsAsync(exception);

		// Act
		await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		_mockLogger.Received(1).Log(
			LogLevel.Error,
			Arg.Any<EventId>(),
			Arg.Is<object>(o => o.ToString()!.Contains("Failed to publish")),
			exception,
			Arg.Any<Func<object, Exception?, string>>()
		);
	}

	[Fact]
	public async Task PublishOrderCreatedAsync_IncludesOrderDetails_InMessage()
	{
		// Arrange
		var orderEvent = new OrderCreatedEvent(
			OrderId: Guid.NewGuid(),
			UserId: "user-123",
			UserEmail: "test@example.com",
			OrderDate: new DateTime(2026, 2, 11, 10, 30, 0, DateTimeKind.Utc),
			TotalAmount: 199.99m,
			ShippingAddress: new ShippingAddressInfo(
				Address: "123 Main St",
				City: "Springfield",
				Province: "IL",
				PostalCode: "62701"
			),
			LineItems: new List<OrderLineItemInfo>
			{
				new(Guid.NewGuid(), "Product A", 2, 49.99m),
				new(Guid.NewGuid(), "Product B", 1, 99.99m)
			}
		);

		ServiceBusMessage? sentMessage = null;
		await _mockSender.SendMessageAsync(
			Arg.Do<ServiceBusMessage>(msg => sentMessage = msg),
			Arg.Any<CancellationToken>()
		);

		// Act
		var result = await _publisher.PublishOrderCreatedAsync(orderEvent, CancellationToken.None);

		// Assert
		Assert.True(result.IsSuccess);

		var envelope = JsonSerializer.Deserialize<MessageEnvelope<OrderCreatedEvent>>(
			sentMessage!.Body.ToString(),
			new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
		);

		Assert.NotNull(envelope);
		Assert.Equal("user-123", envelope!.Payload.UserId);
		Assert.Equal("test@example.com", envelope.Payload.UserEmail);
		Assert.Equal(199.99m, envelope.Payload.TotalAmount);
		Assert.Equal(2, envelope.Payload.LineItems.Count);
		Assert.Equal("123 Main St", envelope.Payload.ShippingAddress.Address);
		Assert.Equal("Springfield", envelope.Payload.ShippingAddress.City);
	}

	private static OrderCreatedEvent CreateSampleOrderEvent()
	{
		return new OrderCreatedEvent(
			OrderId: Guid.NewGuid(),
			UserId: "test-user",
			UserEmail: "test@example.com",
			OrderDate: DateTime.UtcNow,
			TotalAmount: 100.00m,
			ShippingAddress: new ShippingAddressInfo(
				Address: "123 Test St",
				City: "Test City",
				Province: "TC",
				PostalCode: "12345"
			),
			LineItems: new List<OrderLineItemInfo>
			{
				new(Guid.NewGuid(), "Test Product", 1, 100.00m)
			}
		);
	}
}
