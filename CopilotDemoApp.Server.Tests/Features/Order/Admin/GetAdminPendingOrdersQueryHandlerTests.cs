using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order;
using CopilotDemoApp.Server.Features.Order.Admin;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Order.Admin;

public class GetAdminPendingOrdersQueryHandlerTests
{
	[Fact]
	public async Task Returns_Only_Pending_Orders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var pendingOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow.AddDays(-2),
			Status = OrderStatus.Pending,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		var processingOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user2",
			UserEmail = "user2@example.com",
			ShippingAddress = "456 Oak Ave",
			ShippingCity = "Vancouver",
			ShippingProvince = "BC",
			ShippingPostalCode = "V6B 1A1",
			OrderDate = DateTime.UtcNow.AddDays(-1),
			Status = OrderStatus.Processing,
			TotalAmount = 129.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 129.99m, Quantity = 1 }
			}
		};

		db.Orders.AddRange(pendingOrder, processingOrder);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetAdminPendingOrdersQueryHandler(db);
		var query = new GetAdminPendingOrdersQuery(1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(1, response.TotalCount);
		Assert.Single(response.Orders);
		Assert.Equal(pendingOrder.Id, response.Orders[0].Id);
		Assert.Equal(OrderStatus.Pending, response.Orders[0].Status);
	}

	[Fact]
	public async Task Returns_Orders_Sorted_By_OrderDate_Ascending()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var oldestOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow.AddDays(-5),
			Status = OrderStatus.Pending,
			TotalAmount = 59.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 1 }
			}
		};

		var middleOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user2",
			UserEmail = "user2@example.com",
			ShippingAddress = "456 Oak Ave",
			ShippingCity = "Vancouver",
			ShippingProvince = "BC",
			ShippingPostalCode = "V6B 1A1",
			OrderDate = DateTime.UtcNow.AddDays(-3),
			Status = OrderStatus.Pending,
			TotalAmount = 129.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 129.99m, Quantity = 1 }
			}
		};

		var newestOrder = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user3",
			UserEmail = "user3@example.com",
			ShippingAddress = "789 Pine Rd",
			ShippingCity = "Calgary",
			ShippingProvince = "AB",
			ShippingPostalCode = "T2P 1J9",
			OrderDate = DateTime.UtcNow.AddDays(-1),
			Status = OrderStatus.Pending,
			TotalAmount = 89.99m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 3", ProductPrice = 89.99m, Quantity = 1 }
			}
		};

		db.Orders.AddRange(newestOrder, oldestOrder, middleOrder);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetAdminPendingOrdersQueryHandler(db);
		var query = new GetAdminPendingOrdersQuery(1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(3, response.TotalCount);
		Assert.Equal(3, response.Orders.Count);

		// Should be ordered by date ascending (oldest first)
		Assert.Equal(oldestOrder.Id, response.Orders[0].Id);
		Assert.Equal(middleOrder.Id, response.Orders[1].Id);
		Assert.Equal(newestOrder.Id, response.Orders[2].Id);
	}

	[Fact]
	public async Task Returns_Paginated_Results()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		// Create 30 pending orders
		for (int i = 0; i < 30; i++)
		{
			var order = new Database.Order
			{
				Id = Guid.NewGuid(),
				UserId = $"user{i}",
				UserEmail = $"user{i}@example.com",
				ShippingAddress = $"{i} Main St",
				ShippingCity = "Toronto",
				ShippingProvince = "ON",
				ShippingPostalCode = "M5H 2N2",
				OrderDate = DateTime.UtcNow.AddDays(-i),
				Status = OrderStatus.Pending,
				TotalAmount = 59.99m,
				LineItems = new List<Database.OrderLineItem>
				{
					new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product", ProductPrice = 59.99m, Quantity = 1 }
				}
			};
			db.Orders.Add(order);
		}

		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetAdminPendingOrdersQueryHandler(db);

		// Act - Get first page
		var query1 = new GetAdminPendingOrdersQuery(1, 25);
		var result1 = await handler.HandleAsync(query1, TestContext.Current.CancellationToken);

		// Assert - First page
		Assert.True(result1.IsSuccess);
		var response1 = result1.Value!;
		Assert.Equal(30, response1.TotalCount);
		Assert.Equal(25, response1.Orders.Count);
		Assert.Equal(1, response1.Page);
		Assert.Equal(25, response1.PageSize);
		Assert.Equal(2, response1.TotalPages);

		// Act - Get second page
		var query2 = new GetAdminPendingOrdersQuery(2, 25);
		var result2 = await handler.HandleAsync(query2, TestContext.Current.CancellationToken);

		// Assert - Second page
		Assert.True(result2.IsSuccess);
		var response2 = result2.Value!;
		Assert.Equal(30, response2.TotalCount);
		Assert.Equal(5, response2.Orders.Count);
		Assert.Equal(2, response2.Page);
		Assert.Equal(25, response2.PageSize);
		Assert.Equal(2, response2.TotalPages);
	}

	[Fact]
	public async Task Returns_Empty_List_When_No_Pending_Orders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var handler = new GetAdminPendingOrdersQueryHandler(db);
		var query = new GetAdminPendingOrdersQuery(1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(0, response.TotalCount);
		Assert.Empty(response.Orders);
		Assert.Equal(1, response.Page);
		Assert.Equal(25, response.PageSize);
		Assert.Equal(0, response.TotalPages);
	}

	[Fact]
	public async Task Uses_Default_Values_For_Invalid_Page_Parameters()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var order = new Database.Order
		{
			Id = Guid.NewGuid(),
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

		var handler = new GetAdminPendingOrdersQueryHandler(db);
		var query = new GetAdminPendingOrdersQuery(0, 0);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(1, response.Page); // Default to page 1
		Assert.Equal(25, response.PageSize); // Default to 25
	}

	[Fact]
	public async Task Includes_Order_Line_Items()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var order = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = "user1",
			UserEmail = "user1@example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Toronto",
			ShippingProvince = "ON",
			ShippingPostalCode = "M5H 2N2",
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = 179.97m,
			LineItems = new List<Database.OrderLineItem>
			{
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", ProductPrice = 59.99m, Quantity = 2 },
				new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", ProductPrice = 29.99m, Quantity = 2 }
			}
		};

		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetAdminPendingOrdersQueryHandler(db);
		var query = new GetAdminPendingOrdersQuery(1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Single(response.Orders);
		var returnedOrder = response.Orders[0];
		Assert.Equal(2, returnedOrder.LineItems.Count);
		Assert.Equal("Product 1", returnedOrder.LineItems[0].ProductName);
		Assert.Equal("Product 2", returnedOrder.LineItems[1].ProductName);
	}
}
