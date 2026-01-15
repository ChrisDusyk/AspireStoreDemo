using CopilotDemoApp.Server.Features.Order;

namespace CopilotDemoApp.Server.DTOs;

// Product DTOs
public record ProductCreateRequest(string Name, string Description, decimal Price, bool IsActive);
public record ProductUpdateRequest(string Name, string Description, decimal Price, bool IsActive);

// Order DTOs
public record OrderCreateRequest(
	string ShippingAddress,
	string ShippingCity,
	string ShippingState,
	string ShippingPostalCode,
	List<CreateOrderLineItemDto> LineItems
);
