using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public class GetProcessingQueueOrdersQueryHandler(AppDbContext context) : IQueryHandler<GetProcessingQueueOrdersQuery, PagedOrderResponse>
{
	public async Task<Result<PagedOrderResponse>> HandleAsync(GetProcessingQueueOrdersQuery query, CancellationToken ct = default)
	{
		try
		{
			var ordersQuery = context.Orders
				.Include(o => o.LineItems)
				.AsQueryable();

			// Apply status filter if provided
			if (query.Status.HasValue)
			{
				ordersQuery = ordersQuery.Where(o => o.Status == query.Status.Value);
			}

			// Apply user email filter if provided (partial match, case-insensitive)
			if (!string.IsNullOrWhiteSpace(query.UserEmail))
			{
				ordersQuery = ordersQuery.Where(o => o.UserEmail.ToLower().Contains(query.UserEmail.ToLower()));
			}

			// Apply sorting
			ordersQuery = query.SortBy.ToLower() switch
			{
				"status" => query.SortDescending
					? ordersQuery.OrderByDescending(o => o.Status)
					: ordersQuery.OrderBy(o => o.Status),
				"useremail" => query.SortDescending
					? ordersQuery.OrderByDescending(o => o.UserEmail)
					: ordersQuery.OrderBy(o => o.UserEmail),
				_ => query.SortDescending
					? ordersQuery.OrderByDescending(o => o.OrderDate)
					: ordersQuery.OrderBy(o => o.OrderDate)
			};

			var totalCount = await ordersQuery.CountAsync(ct);

			var orderEntities = await ordersQuery
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.ToListAsync(ct);

			var domainOrders = orderEntities.Select(o => new Features.Order.Order(
				o.Id,
				o.UserId,
				o.UserEmail,
				o.ShippingAddress,
				o.ShippingCity,
				o.ShippingProvince,
				o.ShippingPostalCode,
				o.OrderDate,
				o.Status,
				o.TrackingNumber,
				o.TotalAmount,
				[.. o.LineItems.Select(li => new Features.Order.OrderLineItem(
					li.Id,
					li.OrderId,
					li.ProductId,
					li.ProductName,
					li.ProductPrice,
					li.Quantity
				))]
			)).ToList();

			var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

			return Result<PagedOrderResponse>.Success(new PagedOrderResponse(
				domainOrders,
				totalCount,
				query.Page,
				query.PageSize,
				totalPages
			));
		}
		catch (Exception ex)
		{
			return Result<PagedOrderResponse>.Failure(new Error(ErrorCodes.DatabaseError, "Failed to retrieve processing queue orders.", ex));
		}
	}
}
