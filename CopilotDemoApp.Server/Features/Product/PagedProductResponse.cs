using System.Collections.Generic;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record PagedProductResponse(
	IReadOnlyList<ProductResponse> Products,
	int TotalCount,
	int Page,
	int PageSize,
	int TotalPages
);
