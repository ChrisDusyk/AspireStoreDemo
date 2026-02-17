namespace CopilotDemoApp.Shared.Messages;

/// <summary>
/// Event published when a new order is successfully created.
/// Contains order summary data excluding sensitive payment information.
/// </summary>
public sealed record OrderCreatedEvent(
	Guid OrderId,
	string UserId,
	string UserEmail,
	DateTimeOffset OrderDate,
	decimal TotalAmount,
	ShippingAddressInfo ShippingAddress,
	List<OrderLineItemInfo> LineItems
);

/// <summary>
/// Shipping address information for order events.
/// </summary>
public sealed record ShippingAddressInfo(
	string Address,
	string City,
	string Province,
	string PostalCode
);

/// <summary>
/// Line item information for order events.
/// </summary>
public sealed record OrderLineItemInfo(
	Guid ProductId,
	string ProductName,
	int Quantity,
	decimal Price
);
