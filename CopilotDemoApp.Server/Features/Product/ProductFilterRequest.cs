namespace CopilotDemoApp.Server.Features.Product;

public sealed record ProductFilterRequest(
	string? Name,
	bool? IsActive,
	int Page = 1,
	int PageSize = 25
);
