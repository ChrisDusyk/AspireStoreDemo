using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order;
using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Server.Shared.Messages;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace CopilotDemoApp.Server.Tests.Features.Order;

public class CreateOrderCommandHandlerTests
{
	private static IOrderMessagePublisher CreateMockPublisher()
	{
		var mockPublisher = Substitute.For<IOrderMessagePublisher>();
		mockPublisher.PublishOrderCreatedAsync(Arg.Any<OrderCreatedEvent>(), Arg.Any<CancellationToken>())
			.Returns(Result<Unit>.Success(Unit.Value));
		return mockPublisher;
	}

	[Fact]
	public async Task Creates_Order_With_Valid_Command()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product 1", 29.99m, 2),
				new(Guid.NewGuid(), "Test Product 2", 49.99m, 1)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var order = result.Value!;
		Assert.NotEqual(Guid.Empty, order.Id);
		Assert.Equal("user123", order.UserId);
		Assert.Equal("test@example.com", order.UserEmail);
		Assert.Equal("123 Main St", order.ShippingAddress);
		Assert.Equal("Toronto", order.ShippingCity);
		Assert.Equal("ON", order.ShippingProvince);
		Assert.Equal("M5H 2N2", order.ShippingPostalCode);
		Assert.Equal(OrderStatus.Pending, order.Status);
		Assert.Equal(109.97m, order.TotalAmount);
		Assert.Equal(2, order.LineItems.Count);

		// Verify saved to database
		var savedOrder = await db.Orders.Include(o => o.LineItems).FirstOrDefaultAsync(TestContext.Current.CancellationToken);
		Assert.NotNull(savedOrder);
		Assert.Equal(order.Id, savedOrder.Id);
		Assert.Equal(2, savedOrder.LineItems.Count);
	}

	[Fact]
	public async Task Returns_Failure_When_ShippingAddress_Missing()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product 1", 29.99m, 2)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal("ValidationFailed", result.Error!.Code);
		Assert.Equal("Shipping address is required", result.Error!.Message);
	}

	[Fact]
	public async Task Returns_Failure_When_ShippingCity_Missing()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product 1", 29.99m, 2)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal("ValidationFailed", result.Error!.Code);
		Assert.Equal("Shipping city is required", result.Error!.Message);
	}

	[Fact]
	public async Task Returns_Failure_When_ShippingProvince_Missing()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product 1", 29.99m, 2)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal("ValidationFailed", result.Error!.Code);
		Assert.Equal("Shipping province is required", result.Error!.Message);
	}

	[Fact]
	public async Task Returns_Failure_When_PostalCode_Missing()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product 1", 29.99m, 2)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal("ValidationFailed", result.Error!.Code);
		Assert.Equal("Shipping postal code is required", result.Error!.Message);
	}

	[Fact]
	public async Task Returns_Failure_When_LineItems_Empty()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>(),
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal("ValidationFailed", result.Error!.Code);
		Assert.Equal("Order must contain at least one item", result.Error.Message);
	}

	[Fact]
	public async Task Calculates_TotalAmount_Correctly()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Product 1", 10.50m, 3),  // 31.50
				new(Guid.NewGuid(), "Product 2", 25.00m, 2),  // 50.00
				new(Guid.NewGuid(), "Product 3", 7.99m, 5)    // 39.95
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(121.45m, result.Value!.TotalAmount);
	}

	[Fact]
	public async Task Sets_OrderStatus_To_Pending()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(Guid.NewGuid(), "Test Product", 29.99m, 1)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(OrderStatus.Pending, result.Value!.Status);
	}

	[Fact]
	public async Task Preserves_Product_Snapshot_In_LineItems()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var mockPublisher = CreateMockPublisher();
		var handler = new CreateOrderCommandHandler(db, mockPublisher);

		var productId = Guid.NewGuid();
		var command = new CreateOrderCommand(
			UserId: "user123",
			UserEmail: "test@example.com",
			ShippingAddress: "123 Main St",
			ShippingCity: "Toronto",
			ShippingProvince: "ON",
			ShippingPostalCode: "M5H 2N2",
			LineItems: new List<CreateOrderLineItemDto>
			{
				new(productId, "Original Product Name", 99.99m, 2)
			},
			CardNumber: "4111111111111111",
			CardholderName: "Test User",
			ExpiryDate: "12/28",
			Cvv: "123"
		);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var lineItem = result.Value!.LineItems.First();
		Assert.Equal(productId, lineItem.ProductId);
		Assert.Equal("Original Product Name", lineItem.ProductName);
		Assert.Equal(99.99m, lineItem.ProductPrice);
		Assert.Equal(2, lineItem.Quantity);
	}
}
