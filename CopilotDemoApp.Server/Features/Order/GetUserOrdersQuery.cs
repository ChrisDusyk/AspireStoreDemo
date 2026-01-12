using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order;

public record GetUserOrdersQuery(string UserId) : IQuery<List<Order>>;
