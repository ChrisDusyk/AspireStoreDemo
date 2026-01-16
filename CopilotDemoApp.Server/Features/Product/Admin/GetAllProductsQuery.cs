using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public sealed record AdminProductFilterRequest(
	string? Name,
	bool? IsActive,
	string? SortBy,
	string? SortDirection,
	int Page,
	int PageSize
);

public sealed record GetAllProductsQuery(AdminProductFilterRequest Filter) : IQuery<PagedProductResponse>;
