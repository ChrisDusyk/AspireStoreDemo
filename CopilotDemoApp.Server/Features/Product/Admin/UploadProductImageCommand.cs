using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public sealed record UploadProductImageCommand(
	Guid ProductId,
	Stream ImageStream,
	string FileName,
	string ContentType
) : ICommand<ProductResponse>;
