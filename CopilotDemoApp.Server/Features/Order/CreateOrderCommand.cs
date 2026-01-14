using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Order;

public record CreateOrderCommand(
	string UserId,
	string UserEmail,
	string ShippingAddress,
	string ShippingCity,
	string ShippingProvince,
	string ShippingPostalCode,
	List<CreateOrderLineItemDto> LineItems
) : ICommand<Order>;

public record CreateOrderLineItemDto(
	Guid ProductId,
	string ProductName,
	decimal ProductPrice,
	int Quantity
);
