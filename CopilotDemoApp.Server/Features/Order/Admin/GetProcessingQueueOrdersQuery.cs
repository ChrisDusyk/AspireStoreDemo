using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public record GetProcessingQueueOrdersQuery(
	OrderStatus? Status,
	string? UserEmail,
	string SortBy,
	bool SortDescending,
	int Page,
	int PageSize
) : IQuery<PagedOrderResponse>;
