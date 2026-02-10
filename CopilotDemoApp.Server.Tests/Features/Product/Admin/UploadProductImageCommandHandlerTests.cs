using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Features.Product.Admin;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace CopilotDemoApp.Server.Tests.Features.Product.Admin;

public class UploadProductImageCommandHandlerTests
{
	[Fact]
	public async Task HandleAsync_ValidImage_UploadsAndUpdatesProduct()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var imageService = Substitute.For<IProductImageService>();
		var imageUrl = "https://storage.blob.core.windows.net/product-images/test-image.jpg";
		imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<string>.Success(imageUrl));

		using var context = new AppDbContext(options);
		var productId = Guid.NewGuid();
		context.Products.Add(new Database.Product
		{
			Id = productId,
			Name = "Test Product",
			Description = "Description",
			Price = 10.99m,
			IsActive = true,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		});
		await context.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UploadProductImageCommandHandler(context, imageService);
		using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		var command = new UploadProductImageCommand(productId, stream, "test.jpg", "image/jpeg");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		Assert.NotNull(result.Value);
		Assert.Equal(imageUrl, result.Value!.ImageUrl);

		var product = await context.Products.FindAsync([productId], TestContext.Current.CancellationToken);
		Assert.NotNull(product);
		Assert.Equal(imageUrl, product.ImageUrl);
	}

	[Fact]
	public async Task HandleAsync_ProductNotFound_ReturnsNotFoundError()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var imageService = Substitute.For<IProductImageService>();
		using var context = new AppDbContext(options);

		var handler = new UploadProductImageCommandHandler(context, imageService);
		using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		var command = new UploadProductImageCommand(Guid.NewGuid(), stream, "test.jpg", "image/jpeg");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.NotNull(result.Error);
		Assert.Equal(ErrorCodes.NotFound, result.Error!.Code);
	}

	[Fact]
	public async Task HandleAsync_InvalidFileType_ReturnsInvalidFileTypeError()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var imageService = Substitute.For<IProductImageService>();
		using var context = new AppDbContext(options);
		var productId = Guid.NewGuid();
		context.Products.Add(new Database.Product
		{
			Id = productId,
			Name = "Test Product",
			Description = "Description",
			Price = 10.99m,
			IsActive = true,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		});
		await context.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UploadProductImageCommandHandler(context, imageService);
		using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		var command = new UploadProductImageCommand(productId, stream, "test.pdf", "application/pdf");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.NotNull(result.Error);
		Assert.Equal(ErrorCodes.InvalidFileType, result.Error!.Code);
	}

	[Fact]
	public async Task HandleAsync_FileTooLarge_ReturnsFileTooLargeError()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var imageService = Substitute.For<IProductImageService>();
		using var context = new AppDbContext(options);
		var productId = Guid.NewGuid();
		context.Products.Add(new Database.Product
		{
			Id = productId,
			Name = "Test Product",
			Description = "Description",
			Price = 10.99m,
			IsActive = true,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		});
		await context.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UploadProductImageCommandHandler(context, imageService);
		// Create a stream larger than 5MB
		var largeData = new byte[6 * 1024 * 1024];
		using var stream = new MemoryStream(largeData);
		var command = new UploadProductImageCommand(productId, stream, "test.jpg", "image/jpeg");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.False(result.IsSuccess);
		Assert.NotNull(result.Error);
		Assert.Equal(ErrorCodes.FileTooLarge, result.Error!.Code);
	}

	[Fact]
	public async Task HandleAsync_ExistingImage_DeletesOldImage()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		var imageService = Substitute.For<IProductImageService>();
		var oldImageUrl = "https://storage.blob.core.windows.net/product-images/old-image.jpg";
		var newImageUrl = "https://storage.blob.core.windows.net/product-images/new-image.jpg";

		imageService.DeleteAsync(oldImageUrl, Arg.Any<CancellationToken>())
			.Returns(Result<Unit>.Success(Unit.Value));
		imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
			.Returns(Result<string>.Success(newImageUrl));

		using var context = new AppDbContext(options);
		var productId = Guid.NewGuid();
		context.Products.Add(new Database.Product
		{
			Id = productId,
			Name = "Test Product",
			Description = "Description",
			Price = 10.99m,
			IsActive = true,
			ImageUrl = oldImageUrl,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow
		});
		await context.SaveChangesAsync(TestContext.Current.CancellationToken);

		var handler = new UploadProductImageCommandHandler(context, imageService);
		using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
		var command = new UploadProductImageCommand(productId, stream, "test.jpg", "image/jpeg");

		// Act
		var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(result.IsSuccess);
		await imageService.Received(1).DeleteAsync(oldImageUrl, Arg.Any<CancellationToken>());
		Assert.Equal(newImageUrl, result.Value!.ImageUrl);
	}
}
