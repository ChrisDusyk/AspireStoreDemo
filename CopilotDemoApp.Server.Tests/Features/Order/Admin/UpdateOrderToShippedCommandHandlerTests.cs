using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;
using DbOrder = CopilotDemoApp.Server.Database.Order;
using DbOrderLineItem = CopilotDemoApp.Server.Database.OrderLineItem;

namespace CopilotDemoApp.Server.Tests.Features.Order.Admin;

public class UpdateOrderToShippedCommandHandlerTests
{
	[Fact]
	public async Task UpdateOrderToShipped_WithValidProcessingOrder_ReturnsSuccess()
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

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(orderId, "TRACK123456");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(Unit.Value, result.Value);

		var updatedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(updatedOrder);
		Assert.Equal(OrderStatus.Shipped, updatedOrder.Status);
		Assert.Equal("TRACK123456", updatedOrder.TrackingNumber);
	}

	[Fact]
	public async Task UpdateOrderToShipped_WithNonexistentOrder_ReturnsNotFound()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(Guid.NewGuid(), "TRACK123456");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
	}

	[Fact]
	public async Task UpdateOrderToShipped_WithPendingOrder_ReturnsValidationFailed()
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

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(orderId, "TRACK123456");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Processing", result.Error!.Message);
	}

	[Fact]
	public async Task UpdateOrderToShipped_WithShippedOrder_ReturnsValidationFailed()
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

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(orderId, "TRACK123456");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Processing", result.Error!.Message);
	}

	[Fact]
	public async Task UpdateOrderToShipped_WithDeliveredOrder_ReturnsValidationFailed()
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

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(orderId, "TRACK123456");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Processing", result.Error!.Message);
	}

	[Fact]
	public async Task UpdateOrderToShipped_WithEmptyTrackingNumber_ReturnsValidationFailed()
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

		var handler = new UpdateOrderToShippedCommandHandler(db);
		var command = new UpdateOrderToShippedCommand(orderId, "");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Tracking number", result.Error!.Message);
	}
}
