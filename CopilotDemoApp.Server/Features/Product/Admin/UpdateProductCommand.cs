using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public sealed record UpdateProductCommand(
	Guid Id,
	string Name,
	string Description,
	decimal Price,
	bool IsActive
) : ICommand<ProductResponse>;
