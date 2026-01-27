using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order;
using CopilotDemoApp.Server.Features.Order.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Order.Admin;

public class AcceptOrderForFulfillmentCommandHandlerTests
{
	[Fact]
	public async Task Successfully_Accepts_Pending_Order()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new Database.Order
		{
			Id = orderId,
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new AcceptOrderForFulfillmentCommandHandler(db);
		var command = new AcceptOrderForFulfillmentCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);

		// Verify database was updated
		var updatedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(updatedOrder);
		Assert.Equal(OrderStatus.Processing, updatedOrder.Status);
	}

	[Fact]
	public async Task Returns_NotFound_When_Order_Does_Not_Exist()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var handler = new AcceptOrderForFulfillmentCommandHandler(db);
		var nonExistentOrderId = Guid.NewGuid();
		var command = new AcceptOrderForFulfillmentCommand(nonExistentOrderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
		Assert.Contains(nonExistentOrderId.ToString(), result.Error.Message);
	}

	[Fact]
	public async Task Returns_ValidationFailed_When_Order_Is_Not_Pending()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new Database.Order
		{
			Id = orderId,
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Processing, // Already processing
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new AcceptOrderForFulfillmentCommandHandler(db);
		var command = new AcceptOrderForFulfillmentCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);
		Assert.Contains("Pending", result.Error.Message);
		Assert.Contains("Processing", result.Error.Message);

		// Verify database was NOT updated
		var unchangedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(unchangedOrder);
		Assert.Equal(OrderStatus.Processing, unchangedOrder.Status);
	}

	[Fact]
	public async Task Returns_ValidationFailed_When_Order_Is_Shipped()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new Database.Order
		{
			Id = orderId,
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Shipped,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new AcceptOrderForFulfillmentCommandHandler(db);
		var command = new AcceptOrderForFulfillmentCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);

		// Verify database was NOT updated
		var unchangedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(unchangedOrder);
		Assert.Equal(OrderStatus.Shipped, unchangedOrder.Status);
	}

	[Fact]
	public async Task Returns_ValidationFailed_When_Order_Is_Delivered()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var order = new Database.Order
		{
			Id = orderId,
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Delivered,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new AcceptOrderForFulfillmentCommandHandler(db);
		var command = new AcceptOrderForFulfillmentCommand(orderId);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error!.Code);

		// Verify database was NOT updated
		var unchangedOrder = await db.Orders.FindAsync([orderId], TestContext.Current.CancellationToken);
		Assert.NotNull(unchangedOrder);
		Assert.Equal(OrderStatus.Delivered, unchangedOrder.Status);
	}
}
