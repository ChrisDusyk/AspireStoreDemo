using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product;

public class ProductImageService(BlobServiceClient blobServiceClient, ILogger<ProductImageService> logger) : IProductImageService
{
	private const string ContainerName = "product-images";

	public async Task<Result<string>> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default)
	{
		try
		{
			var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
			await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

			// Generate unique blob name to avoid collisions
			var blobName = $"{Guid.NewGuid()}_{fileName}";
			var blobClient = containerClient.GetBlobClient(blobName);

			var blobHttpHeaders = new BlobHttpHeaders
			{
				ContentType = contentType
			};

			await blobClient.UploadAsync(stream, new BlobUploadOptions
			{
				HttpHeaders = blobHttpHeaders
			}, cancellationToken);

			logger.LogInformation("Uploaded blob {BlobName} to container {Container}", blobName, ContainerName);

			return Result<string>.Success(blobClient.Uri.ToString());
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to upload blob to container {Container}", ContainerName);
			return Result<string>.Failure(new Error("BlobUploadFailed", "Failed to upload image to storage", ex));
		}
	}

	public async Task<Result<Unit>> DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
	{
		try
		{
			// Extract blob name from URL
			var uri = new Uri(imageUrl);
			var blobName = uri.Segments[^1]; // Get last segment (blob name)

			var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
			var blobClient = containerClient.GetBlobClient(blobName);

			await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

			logger.LogInformation("Deleted blob {BlobName} from container {Container}", blobName, ContainerName);

			return Result<Unit>.Success(Unit.Value);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Failed to delete blob from container {Container}", ContainerName);
			return Result<Unit>.Failure(new Error("BlobDeleteFailed", "Failed to delete image from storage", ex));
		}
	}
}
