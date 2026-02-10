using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Product.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Tests.Features.Product.Admin;

public class CreateProductCommandHandlerTests
{
	[Fact]
	public async Task HandleAsync_ValidCommand_ReturnsSuccessAndCreatesProduct()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		using var context = new AppDbContext(options);
		var handler = new CreateProductCommandHandler(context);
		var command = new CreateProductCommand("Test Product", "Description", 10.99m, true);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var productId = result.Value;
		Assert.NotEqual(Guid.Empty, productId);

		var product = await context.Products.FindAsync([productId], TestContext.Current.CancellationToken);
		Assert.NotNull(product);
		Assert.Equal("Test Product", product.Name);
		Assert.Equal(10.99m, product.Price);
		Assert.True(product.IsActive);
		Assert.Null(product.ImageUrl);
	}

	[Fact]
	public async Task HandleAsync_WithImageUrl_CreatesProductWithImage()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		using var context = new AppDbContext(options);
		var handler = new CreateProductCommandHandler(context);
		var imageUrl = "https://storage.blob.core.windows.net/product-images/test.jpg";
		var command = new CreateProductCommand("Test Product", "Description", 10.99m, true, imageUrl);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		var productId = result.Value;
		
		var product = await context.Products.FindAsync([productId], TestContext.Current.CancellationToken);
		Assert.NotNull(product);
		Assert.Equal(imageUrl, product.ImageUrl);
	}

	[Fact]
	public async Task HandleAsync_InvalidName_ReturnsValidationFailure()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		using var context = new AppDbContext(options);
		var handler = new CreateProductCommandHandler(context);
		var command = new CreateProductCommand("", "Description", 10.99m, true);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.NotNull(result.Error);

		Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
	}

	[Fact]
	public async Task HandleAsync_InvalidPrice_ReturnsValidationFailure()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		using var context = new AppDbContext(options);
		var handler = new CreateProductCommandHandler(context);
		var command = new CreateProductCommand("Test Product", "Description", 0m, true);

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.NotNull(result.Error);
		Assert.Equal(ErrorCodes.ValidationFailed, result.Error.Code);
	}
}
