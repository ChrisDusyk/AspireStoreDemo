using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Product;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Product;

public class GetProductByIdQueryHandlerTests
{
	[Fact]
	public async Task Returns_Product_When_Found()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new CopilotDemoApp.Server.Database.AppDbContext(options);
		var entity = new CopilotDemoApp.Server.Database.Product
		{
			Id = Guid.NewGuid(),
			Name = "Test Product",
			Description = "Test Desc",
			Price = 42.0m,
			IsActive = true,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		};
		db.Products.Add(entity);
		await db.SaveChangesAsync(TestContext.Current.CancellationToken);
		var handler = new GetProductByIdQueryHandler(db);
		// Act
		var result = await handler.Handle(new GetProductByIdQuery(entity.Id), CancellationToken.None);
		// Assert
		Assert.True(result.IsSuccess);
		var opt = result.Value;
		Assert.True(opt.HasValue);
		Assert.Equal(entity.Id, opt.Value.Id);
		Assert.Equal(entity.Name, opt.Value.Name);
	}

	[Fact]
	public async Task Returns_None_When_Not_Found()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		var db = new CopilotDemoApp.Server.Database.AppDbContext(options);
		var handler = new GetProductByIdQueryHandler(db);
		// Act
		var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);
		// Assert
		Assert.True(result.IsSuccess);
		Assert.False(result.Value.HasValue);
	}
}
