using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public class UploadProductImageCommandHandler(
	AppDbContext db,
	IProductImageService imageService) : ICommandHandler<UploadProductImageCommand, ProductResponse>
{
	private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
	private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

	public async Task<Result<ProductResponse>> HandleAsync(UploadProductImageCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			// Validate content type
			if (!AllowedContentTypes.Contains(command.ContentType.ToLowerInvariant()))
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.InvalidFileType, $"File type '{command.ContentType}' is not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}")
				);
			}

			// Validate file size
			if (command.ImageStream.Length > MaxFileSizeBytes)
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.FileTooLarge, $"File size exceeds the maximum allowed size of {MaxFileSizeBytes / 1024 / 1024}MB")
				);
			}

			// Fetch product by ID
			var entity = await db.Products.FindAsync(new object[] { command.ProductId }, cancellationToken);
			if (entity is null)
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.NotFound, $"Product with ID {command.ProductId} not found.")
				);
			}

			// Delete old image if exists
			if (!string.IsNullOrWhiteSpace(entity.ImageUrl))
			{
				var deleteResult = await imageService.DeleteAsync(entity.ImageUrl, cancellationToken);
				// Log but don't fail if delete fails - the old image might already be gone
				if (!deleteResult.IsSuccess)
				{
					// Continue anyway - we'll just have an orphaned blob
				}
			}

			// Upload new image
			var uploadResult = await imageService.UploadAsync(
				command.ImageStream,
				command.FileName,
				command.ContentType,
				cancellationToken
			);

			if (!uploadResult.IsSuccess)
			{
				return Result<ProductResponse>.Failure(uploadResult.Error!);
			}

			// Update product with new image URL
			entity.ImageUrl = uploadResult.Value;
			entity.UpdatedDate = DateTime.UtcNow;

			await db.SaveChangesAsync(cancellationToken);

			// Map entity to domain Product, then to ProductResponse
			var domain = ProductMapper.MapEntityToDomain(entity);
			var response = ProductResponseMapper.MapDomainToResponse(domain);

			return Result<ProductResponse>.Success(response);
		}
		catch (Exception ex)
		{
			return Result<ProductResponse>.Failure(
				new Error(ErrorCodes.DatabaseError, "An error occurred while uploading the product image.", ex)
			);
		}
	}
}
