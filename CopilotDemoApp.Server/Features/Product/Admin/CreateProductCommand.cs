using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public record CreateProductCommand(
	string Name,
	string Description,
	decimal Price,
	bool IsActive,
	string? ImageUrl = null
) : ICommand<Guid>;
