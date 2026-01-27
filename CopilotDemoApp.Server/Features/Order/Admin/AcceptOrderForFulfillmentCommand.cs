using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public sealed record AcceptOrderForFulfillmentCommand(Guid OrderId) : ICommand<Unit>;
