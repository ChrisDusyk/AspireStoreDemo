using System;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record ProductResponse(
	Guid Id,
	string? Name,
	string? Description,
	decimal? Price,
	bool IsActive,
	DateTime CreatedDate,
	DateTime UpdatedDate
);
