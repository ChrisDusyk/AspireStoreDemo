using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;
using DbOrder = CopilotDemoApp.Server.Database.Order;
using DbOrderLineItem = CopilotDemoApp.Server.Database.OrderLineItem;

namespace CopilotDemoApp.Server.Tests.Features.Order.Admin;

public class UpdateOrderToDeliveredCommandHandlerTests
{
	[Fact]
	public async Task UpdateOrderToDelivered_WithValidShippedOrder_ReturnsSuccess()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new DbOrder
		{
			Id = orderId,
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Springfield",
			ShippingProvince = "IL",
			ShippingPostalCode = "62701",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Shipped,
			TotalAmount = 100.00m,
			LineItems = new List<DbOrderLineItem>()
		};
		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UpdateOrderToDeliveredCommandHandler(db);
		var command = new UpdateOrderToDeliveredCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(Unit.Value, result.Value);

		var updatedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(updatedOrder);
		Assert.Equal(OrderStatus.Delivered, updatedOrder.Status);
	}

	[Fact]
	public async Task UpdateOrderToDelivered_WithNonexistentOrder_ReturnsNotFound()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var handler = new UpdateOrderToDeliveredCommandHandler(db);
		var command = new UpdateOrderToDeliveredCommand(Guid.NewGuid());

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
	}

	[Fact]
	public async Task UpdateOrderToDelivered_WithPendingOrder_ReturnsValidationFailed()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new DbOrder
		{
			Id = orderId,
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Springfield",
			ShippingProvince = "IL",
			ShippingPostalCode = "62701",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = 100.00m,
			LineItems = new List<DbOrderLineItem>()
		};
		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UpdateOrderToDeliveredCommandHandler(db);
		var command = new UpdateOrderToDeliveredCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Shipped", result.Error!.Message);
	}

	[Fact]
	public async Task UpdateOrderToDelivered_WithProcessingOrder_ReturnsValidationFailed()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new DbOrder
		{
			Id = orderId,
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Springfield",
			ShippingProvince = "IL",
			ShippingPostalCode = "62701",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Processing,
			TotalAmount = 100.00m,
			LineItems = new List<DbOrderLineItem>()
		};
		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UpdateOrderToDeliveredCommandHandler(db);
		var command = new UpdateOrderToDeliveredCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Shipped", result.Error!.Message);
	}

	[Fact]
	public async Task UpdateOrderToDelivered_WithDeliveredOrder_ReturnsValidationFailed()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new DbOrder
		{
			Id = orderId,
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Springfield",
			ShippingProvince = "IL",
			ShippingPostalCode = "62701",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Delivered,
			TotalAmount = 100.00m,
			LineItems = new List<DbOrderLineItem>()
		};
		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UpdateOrderToDeliveredCommandHandler(db);
		var command = new UpdateOrderToDeliveredCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Shipped", result.Error!.Message);
	}
}
