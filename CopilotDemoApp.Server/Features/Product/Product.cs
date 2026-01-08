using CopilotDemoApp.Server.Shared;
using System;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record Product(
	Guid Id,
	Option<string> Name,
	Option<string> Description,
	Option<decimal> Price,
	bool IsActive,
	DateTime CreatedDate,
	DateTime UpdatedDate
);
