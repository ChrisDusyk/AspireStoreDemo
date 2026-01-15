using CopilotDemoApp.Server.Database;

namespace CopilotDemoApp.Server.Features.Order;

public sealed record Order(
	Guid Id,
	string UserId,
	string UserEmail,
	string ShippingAddress,
	string ShippingCity,
	string ShippingProvince,
	string ShippingPostalCode,
	DateTime OrderDate,
	OrderStatus Status,
	decimal TotalAmount,
	List<OrderLineItem> LineItems
);
