using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public record UpdateOrderToShippedCommand(Guid OrderId) : ICommand<Unit>;
