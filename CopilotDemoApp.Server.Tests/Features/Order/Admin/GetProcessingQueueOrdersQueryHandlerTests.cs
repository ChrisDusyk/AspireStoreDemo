using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Order.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;
using DbOrder = CopilotDemoApp.Server.Database.Order;
using DbOrderLineItem = CopilotDemoApp.Server.Database.OrderLineItem;
using DbProduct = CopilotDemoApp.Server.Database.Product;

namespace CopilotDemoApp.Server.Tests.Features.Order.Admin;

public class GetProcessingQueueOrdersQueryHandlerTests
{
	[Fact]
	public async Task GetProcessingQueueOrders_WithProcessingStatusFilter_ReturnsOnlyProcessingOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "user1@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "user2@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Shipped,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "user3@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, null, "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(2, result.Value!.TotalCount);
		Assert.Equal(2, result.Value!.Orders.Count);
		Assert.All(result.Value!.Orders, o => Assert.Equal(OrderStatus.Processing, o.Status));
	}

	[Fact]
	public async Task GetProcessingQueueOrders_WithShippedStatusFilter_ReturnsOnlyShippedOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "user1@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "user2@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Shipped,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Shipped, null, "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(1, result.Value!.TotalCount);
		Assert.Single(result.Value!.Orders);
		Assert.Equal(OrderStatus.Shipped, result.Value!.Orders[0].Status);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_WithUserEmailFilter_ReturnsMatchingOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "alice@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "bob@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Processing,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "alice.smith@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, "alice", "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(2, result.Value!.TotalCount);
		Assert.Equal(2, result.Value!.Orders.Count);
		Assert.All(result.Value!.Orders, o => Assert.Contains("alice", o.UserEmail, StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public async Task GetProcessingQueueOrders_WithUserEmailFilterCaseInsensitive_ReturnsMatchingOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var order = new DbOrder
		{
			Id = Guid.NewGuid(),
			UserId = "user1",
			UserEmail = "Alice@Example.com",
			ShippingAddress = "123 Main St",
			ShippingCity = "Springfield",
			ShippingProvince = "IL",
			ShippingPostalCode = "62701",
			OrderDate = DateTime.UtcNow.AddDays(-3),
			Status = OrderStatus.Processing,
			TotalAmount = 100.00m,
			LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
		};
		db.Orders.Add(order);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, "ALICE", "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(1, result.Value!.TotalCount);
		Assert.Single(result.Value!.Orders);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_SortByOrderDateAscending_ReturnsSortedOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "user1@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "user2@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "user3@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Processing,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, null, "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value!.Orders.Count);
		Assert.True(result.Value!.Orders[0].OrderDate < result.Value!.Orders[1].OrderDate);
		Assert.True(result.Value!.Orders[1].OrderDate < result.Value!.Orders[2].OrderDate);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_SortByOrderDateDescending_ReturnsSortedOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "user1@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "user2@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, null, "OrderDate", true, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(2, result.Value!.Orders.Count);
		Assert.True(result.Value!.Orders[0].OrderDate > result.Value!.Orders[1].OrderDate);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_SortByStatusAscending_ReturnsSortedOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "user1@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Delivered,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "user2@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Processing,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "user3@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Shipped,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(null, null, "Status", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value!.Orders.Count);
		Assert.Equal(OrderStatus.Processing, result.Value!.Orders[0].Status);
		Assert.Equal(OrderStatus.Shipped, result.Value!.Orders[1].Status);
		Assert.Equal(OrderStatus.Delivered, result.Value!.Orders[2].Status);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_SortByUserEmailAscending_ReturnsSortedOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "charlie@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "alice@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Processing,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "bob@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, null, "UserEmail", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(3, result.Value!.Orders.Count);
		Assert.Equal("alice@example.com", result.Value!.Orders[0].UserEmail);
		Assert.Equal("bob@example.com", result.Value!.Orders[1].UserEmail);
		Assert.Equal("charlie@example.com", result.Value!.Orders[2].UserEmail);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_WithPagination_ReturnsCorrectPage()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		for (int i = 0; i < 30; i++)
		{
			var order = new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = $"user{i}",
				UserEmail = $"user{i}@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-i),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			};
			db.Orders.Add(order);
		}
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, null, "OrderDate", false, 2, 10);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(30, result.Value!.TotalCount);
		Assert.Equal(10, result.Value!.Orders.Count);
		Assert.Equal(2, result.Value!.Page);
		Assert.Equal(10, result.Value!.PageSize);
		Assert.Equal(3, result.Value!.TotalPages);
	}

	[Fact]
	public async Task GetProcessingQueueOrders_WithCombinedFilters_ReturnsFilteredOrders()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new AppDbContext(options);

		var product = new DbProduct { Id = Guid.NewGuid(), Name = "Test Product", Price = 10.00m, IsActive = true };
		db.Products.Add(product);

		var orders = new[]
		{
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user1",
				UserEmail = "alice@example.com",
				ShippingAddress = "123 Main St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62701",
				OrderDate = DateTime.UtcNow.AddDays(-3),
				Status = OrderStatus.Processing,
				TotalAmount = 100.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 1, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user2",
				UserEmail = "alice@example.com",
				ShippingAddress = "456 Elm St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62702",
				OrderDate = DateTime.UtcNow.AddDays(-2),
				Status = OrderStatus.Shipped,
				TotalAmount = 200.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 2, ProductPrice = 10.00m } }
			},
			new DbOrder
			{
				Id = Guid.NewGuid(),
				UserId = "user3",
				UserEmail = "bob@example.com",
				ShippingAddress = "789 Oak St",
				ShippingCity = "Springfield",
				ShippingProvince = "IL",
				ShippingPostalCode = "62703",
				OrderDate = DateTime.UtcNow.AddDays(-1),
				Status = OrderStatus.Processing,
				TotalAmount = 300.00m,
				LineItems = new List<DbOrderLineItem> { new() { ProductId = product.Id, Quantity = 3, ProductPrice = 10.00m } }
			}
		};
		db.Orders.AddRange(orders);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new GetProcessingQueueOrdersQueryHandler(db);
		var query = new GetProcessingQueueOrdersQuery(OrderStatus.Processing, "alice", "OrderDate", false, 1, 25);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.Equal(1, result.Value!.TotalCount);
		Assert.Single(result.Value!.Orders);
		Assert.Equal("alice@example.com", result.Value!.Orders[0].UserEmail);
		Assert.Equal(OrderStatus.Processing, result.Value!.Orders[0].Status);
	}
}
