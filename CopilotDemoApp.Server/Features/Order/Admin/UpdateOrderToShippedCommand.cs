using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public record UpdateOrderToShippedCommand(Guid OrderId, string TrackingNumber) : ICommand<Unit>;
