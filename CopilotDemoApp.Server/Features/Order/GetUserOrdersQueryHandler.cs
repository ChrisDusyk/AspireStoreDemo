using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order;

public class GetUserOrdersQueryHandler(AppDbContext context) : IQueryHandler<GetUserOrdersQuery, List<Order>>
{
	public async Task<Result<List<Order>>> HandleAsync(GetUserOrdersQuery query, CancellationToken cancellationToken = default)
	{
		var orderEntities = await context.Orders
			.Include(o => o.LineItems)
			.Where(o => o.UserId == query.UserId)
			.OrderByDescending(o => o.OrderDate)
			.ToListAsync(cancellationToken);

		var domainOrders = orderEntities.Select(o => new Order(
			o.Id,
			o.UserId,
			o.UserEmail,
			o.ShippingAddress,
			o.ShippingCity,
			o.ShippingState,
			o.ShippingPostalCode,
			o.OrderDate,
			o.Status,
			o.TotalAmount,
			o.LineItems.Select(li => new OrderLineItem(
				li.Id,
				li.OrderId,
				li.ProductId,
				li.ProductName,
				li.ProductPrice,
				li.Quantity
			)).ToList()
		)).ToList();

		return Result<List<Order>>.Success(domainOrders);
	}
}
