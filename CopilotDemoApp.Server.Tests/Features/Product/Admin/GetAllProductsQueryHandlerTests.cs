using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Product.Admin;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Product.Admin;

public class GetAllProductsQueryHandlerTests
{
	private static AppDbContext CreateInMemoryContext()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		return new AppDbContext(options);
	}

	private static async Task SeedProducts(AppDbContext db)
	{
		var products = new[]
		{
			new CopilotDemoApp.Server.Database.Product
			{
				Id = Guid.NewGuid(),
				Name = "Active Product A",
				Description = "Description A",
				Price = 10.0m,
				IsActive = true,
				CreatedDate = DateTime.UtcNow.AddDays(-5),
				UpdatedDate = DateTime.UtcNow.AddDays(-1)
			},
			new CopilotDemoApp.Server.Database.Product
			{
				Id = Guid.NewGuid(),
				Name = "Inactive Product B",
				Description = "Description B",
				Price = 20.0m,
				IsActive = false,
				CreatedDate = DateTime.UtcNow.AddDays(-4),
				UpdatedDate = DateTime.UtcNow.AddDays(-2)
			},
			new CopilotDemoApp.Server.Database.Product
			{
				Id = Guid.NewGuid(),
				Name = "Active Product C",
				Description = "Description C",
				Price = 30.0m,
				IsActive = true,
				CreatedDate = DateTime.UtcNow.AddDays(-3),
				UpdatedDate = DateTime.UtcNow.AddDays(-3)
			},
			new CopilotDemoApp.Server.Database.Product
			{
				Id = Guid.NewGuid(),
				Name = "Inactive Product D",
				Description = "Description D",
				Price = 40.0m,
				IsActive = false,
				CreatedDate = DateTime.UtcNow.AddDays(-2),
				UpdatedDate = DateTime.UtcNow
			}
		};
		db.Products.AddRange(products);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);
	}

	[Fact]
	public async Task Returns_All_Products_When_IsActive_Filter_Is_Null()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(4, response.TotalCount);
		Assert.Equal(4, response.Products.Count);
	}

	[Fact]
	public async Task Returns_Only_Active_Products_When_IsActive_Is_True()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, true, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(2, response.TotalCount);
		Assert.All(response.Products, p => Assert.True(p.IsActive));
	}

	[Fact]
	public async Task Returns_Only_Inactive_Products_When_IsActive_Is_False()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, false, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(2, response.TotalCount);
		Assert.All(response.Products, p => Assert.False(p.IsActive));
	}

	[Fact]
	public async Task Filters_By_Name_Case_Insensitive()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest("product a", null, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(1, response.TotalCount);
		Assert.Contains("Active Product A", response.Products[0].Name);
	}

	[Fact]
	public async Task Sorts_By_Name_Ascending_By_Default()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "name", "asc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal("Active Product A", response.Products[0].Name);
		Assert.Equal("Active Product C", response.Products[1].Name);
		Assert.Equal("Inactive Product B", response.Products[2].Name);
		Assert.Equal("Inactive Product D", response.Products[3].Name);
	}

	[Fact]
	public async Task Sorts_By_Name_Descending()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "name", "desc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal("Inactive Product D", response.Products[0].Name);
		Assert.Equal("Inactive Product B", response.Products[1].Name);
		Assert.Equal("Active Product C", response.Products[2].Name);
		Assert.Equal("Active Product A", response.Products[3].Name);
	}

	[Fact]
	public async Task Sorts_By_CreatedDate_Descending()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "createdDate", "desc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		// Most recent first
		Assert.Equal("Inactive Product D", response.Products[0].Name);
		Assert.Equal("Active Product C", response.Products[1].Name);
		Assert.Equal("Inactive Product B", response.Products[2].Name);
		Assert.Equal("Active Product A", response.Products[3].Name);
	}

	[Fact]
	public async Task Sorts_By_CreatedDate_Ascending()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "createdDate", "asc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		// Oldest first
		Assert.Equal("Active Product A", response.Products[0].Name);
		Assert.Equal("Inactive Product B", response.Products[1].Name);
		Assert.Equal("Active Product C", response.Products[2].Name);
		Assert.Equal("Inactive Product D", response.Products[3].Name);
	}

	[Fact]
	public async Task Sorts_By_UpdatedDate_Descending()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "updatedDate", "desc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		// Most recently updated first
		Assert.Equal("Inactive Product D", response.Products[0].Name);
	}

	[Fact]
	public async Task Sorts_By_UpdatedDate_Ascending()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "updatedDate", "asc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		// Least recently updated first
		Assert.Equal("Active Product C", response.Products[0].Name);
	}

	[Fact]
	public async Task Paginates_Results_Correctly()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "name", "asc", 1, 2);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(4, response.TotalCount);
		Assert.Equal(2, response.Products.Count);
		Assert.Equal(2, response.TotalPages);
		Assert.Equal(1, response.Page);
	}

	[Fact]
	public async Task Returns_Second_Page_Correctly()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "name", "asc", 2, 2);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(4, response.TotalCount);
		Assert.Equal(2, response.Products.Count);
		Assert.Equal(2, response.Page);
		Assert.Equal("Inactive Product B", response.Products[0].Name);
		Assert.Equal("Inactive Product D", response.Products[1].Name);
	}

	[Fact]
	public async Task Combines_Name_Filter_And_IsActive_Filter()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest("product", true, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(2, response.TotalCount);
		Assert.All(response.Products, p => Assert.True(p.IsActive));
		Assert.All(response.Products, p => Assert.Contains("Product", p.Name));
	}

	[Fact]
	public async Task Returns_Empty_List_When_No_Products_Match()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest("nonexistent", null, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(0, response.TotalCount);
		Assert.Empty(response.Products);
	}

	[Fact]
	public async Task Defaults_To_Name_Ascending_When_Invalid_Sort_Field()
	{
		// Arrange
		var db = CreateInMemoryContext();
		await SeedProducts(db);
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, "invalid", "asc", 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		// Should default to name ascending
		Assert.Equal("Active Product A", response.Products[0].Name);
		Assert.Equal("Active Product C", response.Products[1].Name);
	}

	[Fact]
	public async Task Handles_Empty_Database()
	{
		// Arrange
		var db = CreateInMemoryContext();
		var handler = new GetAllProductsQueryHandler(db);
		var filter = new AdminProductFilterRequest(null, null, null, null, 1, 25);
		var query = new GetAllProductsQuery(filter);

		// Act
		var result = await handler.HandleAsync(query, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var response = result.Value!;
		Assert.Equal(0, response.TotalCount);
		Assert.Empty(response.Products);
		Assert.Equal(0, response.TotalPages);
	}
}
