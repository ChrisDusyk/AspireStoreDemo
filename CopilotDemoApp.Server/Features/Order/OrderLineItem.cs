namespace CopilotDemoApp.Server.Features.Order;

public sealed record OrderLineItem(
	Guid Id,
	Guid OrderId,
	Guid ProductId,
	string ProductName,
	decimal ProductPrice,
	int Quantity
);
