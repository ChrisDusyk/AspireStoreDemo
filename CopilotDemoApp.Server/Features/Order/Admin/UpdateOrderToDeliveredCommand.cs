using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public record UpdateOrderToDeliveredCommand(Guid OrderId) : ICommand<Unit>;
