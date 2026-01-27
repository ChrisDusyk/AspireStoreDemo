using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public class GetAdminPendingOrdersQueryHandler(AppDbContext context) : IQueryHandler<GetAdminPendingOrdersQuery, PagedOrderResponse>
{
	public async Task<Result<PagedOrderResponse>> HandleAsync(GetAdminPendingOrdersQuery query, CancellationToken cancellationToken = default)
	{
		try
		{
			var ordersQuery = context.Orders
				.Include(o => o.LineItems)
				.Where(o => o.Status == OrderStatus.Pending);

			var totalCount = await ordersQuery.CountAsync(cancellationToken);
			var page = query.Page > 0 ? query.Page : 1;
			var pageSize = query.PageSize > 0 ? query.PageSize : 25;
			var skip = (page - 1) * pageSize;

			var orderEntities = await ordersQuery
				.OrderBy(o => o.OrderDate)
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			var domainOrders = orderEntities.Select(o => new Order(
				o.Id,
				o.UserId,
				o.UserEmail,
				o.ShippingAddress,
				o.ShippingCity,
				o.ShippingProvince,
				o.ShippingPostalCode,
				o.OrderDate,
				o.Status,
				o.TotalAmount,
				[.. o.LineItems.Select(li => new OrderLineItem(
					li.Id,
					li.OrderId,
					li.ProductId,
					li.ProductName,
					li.ProductPrice,
					li.Quantity
				))]
			)).ToList();

			var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

			var pagedResponse = new PagedOrderResponse(
				domainOrders,
				totalCount,
				page,
				pageSize,
				totalPages
			);

			return Result<PagedOrderResponse>.Success(pagedResponse);
		}
		catch (Exception ex)
		{
			return Result<PagedOrderResponse>.Failure(new Error(ErrorCodes.DatabaseError, "An error occurred while retrieving pending orders", ex));
		}
	}
}
