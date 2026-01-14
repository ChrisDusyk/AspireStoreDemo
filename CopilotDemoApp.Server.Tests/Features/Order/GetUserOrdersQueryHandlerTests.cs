using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Order;

public class GetUserOrdersQueryHandlerTests
{
	[Fact]
	public async Task Returns_Orders_For_User()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var order1 = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingState = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow.AddDays(-2),
			Status = OrderStatus.Delivered,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		var order2 = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "456 Oak Ave",
			ShippingCity = "Vancouver",
			ShippingState = "BC",
			ShippingPostalCode = "V6B 1A1",
			OrderDate = DateTime.UtcNow.AddDays(-1),
			Status = OrderStatus.Pending,
			TotalAmount = 129.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 64.995m, Quantity = 2 }
			}
		};

		db.Orders.AddRange(order1, order2);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("user123");

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var orders = result.Value!;
		Assert.Equal(2, orders.Count);
		
		// Should be ordered by date descending
		Assert.Equal(order2.Id, orders[0].Id);
		Assert.Equal(order1.Id, orders[1].Id);
	}

	[Fact]
	public async Task Returns_Empty_List_When_No_Orders_Found()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);
		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("nonexistent-user");

		// Act
	var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Empty(result.Value!);
	}

	[Fact]
	public async Task Does_Not_Return_Other_Users_Orders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var user1Order = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingState = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = 50.00m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 50.00m, Quantity = 1 }
			}
		};

		var user2Order = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user2",
			UserEmail = "user2@example.com",
			ShippingAddress = "456 Oak Ave",
			ShippingCity = "Vancouver",
			ShippingState = "BC",
			ShippingPostalCode = "V6B 1A1",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Processing,
			TotalAmount = 75.00m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 75.00m, Quantity = 1 }
			}
		};

		db.Orders.AddRange(user1Order, user2Order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("user1");

		// Act
	var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

	// Assert
	Assert.True(result.IsSuccess);
	Assert.Single(result.Value!);
	Assert.Equal(user1Order.Id, result.Value![0].Id);
	Assert.Equal("user1", result.Value[0].UserId);
	}

	[Fact]
	public async Task Includes_LineItems_In_Results()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var order = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingState = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = 109.97m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 29.99m, Quantity = 2 },
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 49.99m, Quantity = 1 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("user123");

		// Act
	var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

	// Assert
	Assert.True(result.IsSuccess);
	var returnedOrder = result.Value!.First();
		Assert.Equal(2, returnedOrder.LineItems.Count);
		Assert.Contains(returnedOrder.LineItems, li => li.ProductName == "Product 1" && li.Quantity == 2);
		Assert.Contains(returnedOrder.LineItems, li => li.ProductName == "Product 2" && li.Quantity == 1);
	}

	[Fact]
	public async Task Orders_Results_By_OrderDate_Descending()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var oldOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingState = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow.AddDays(-10),
			Status = OrderStatus.Delivered,
			TotalAmount = 50.00m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Old Product", ProductPrice = 50.00m, Quantity = 1 }
			}
		};

		var recentOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user123",
			UserEmail = "test@example.com",
			ShippingAddress = "456 Oak Ave",
			ShippingCity = "Vancouver",
			ShippingState = "BC",
			ShippingPostalCode = "V6B 1A1",
			OrderDate = DateTime.UtcNow.AddHours(-1),
			Status = OrderStatus.Pending,
			TotalAmount = 75.00m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Recent Product", ProductPrice = 75.00m, Quantity = 1 }
			}
		};

		db.Orders.AddRange(oldOrder, recentOrder);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("user123");

		// Act
	var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
	Assert.Equal(2, result.Value!.Count);
		Assert.Equal(recentOrder.Id, result.Value[0].Id); // Most recent first
		Assert.Equal(oldOrder.Id, result.Value[1].Id);
	}

	[Fact]
	public async Task Maps_All_Order_Properties_Correctly()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var orderId = Guid.NewGuid();
		var orderDate = DateTime.UtcNow;
		var order = new Database.Order
		{
			Id = orderId,
			UserId = "user456",
			UserEmail = "user456@example.com",
			ShippingAddress = "789 Elm St",
			ShippingCity = "Calgary",
			ShippingState = "AB",
			ShippingPostalCode = "T2P 0A1",
			OrderDate = orderDate,
			Status = OrderStatus.Shipped,
			TotalAmount = 199.99m,
			LineItems = new List<Database.OrderLineItem>()
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetUserOrdersQueryHandler(db);
		var query = new GetUserOrdersQuery("user456");

		// Act
	var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

	// Assert
	Assert.True(result.IsSuccess);
	var returnedOrder = result.Value!.First();
		Assert.Equal(orderId, returnedOrder.Id);
		Assert.Equal("user456", returnedOrder.UserId);
		Assert.Equal("user456@example.com", returnedOrder.UserEmail);
		Assert.Equal("789 Elm St", returnedOrder.ShippingAddress);
		Assert.Equal("Calgary", returnedOrder.ShippingCity);
		Assert.Equal("AB", returnedOrder.ShippingState);
		Assert.Equal("T2P 0A1", returnedOrder.ShippingPostalCode);
		Assert.Equal(orderDate, returnedOrder.OrderDate);
		Assert.Equal(OrderStatus.Shipped, returnedOrder.Status);
		Assert.Equal(199.99m, returnedOrder.TotalAmount);
	}
}
