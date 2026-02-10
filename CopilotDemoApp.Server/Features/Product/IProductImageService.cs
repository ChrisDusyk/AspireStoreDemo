using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product;

public interface IProductImageService
{
	Task<Result<string>> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
	Task<Result<Unit>> DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}
