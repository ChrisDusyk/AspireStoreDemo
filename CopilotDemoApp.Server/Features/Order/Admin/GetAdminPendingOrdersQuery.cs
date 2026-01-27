using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public sealed record GetAdminPendingOrdersQuery(int Page, int PageSize) : IQuery<PagedOrderResponse>;

public sealed record PagedOrderResponse(
	IReadOnlyList<Order> Orders,
	int TotalCount,
	int Page,
	int PageSize,
	int TotalPages
);
