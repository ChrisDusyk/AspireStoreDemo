using CopilotDemoApp.Server.Features.Order.Admin;
using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Server.Database;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CopilotDemoApp.Server.Features.Order;

// Order DTOs
public record OrderCreateRequest(
	string ShippingAddress,
	string ShippingCity,
	string ShippingState,
	string ShippingPostalCode,
	List<CreateOrderLineItemDto> LineItems,
	string CardNumber,
	string CardholderName,
	string ExpiryDate,
	string Cvv
);

public static class OrderEndpoints
{
	public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
	{
		var api = app.MapGroup("/api");

		// User endpoints for orders
		var orders = api.MapGroup("/orders")
			.RequireAuthorization("UserAccess");

		orders.MapGet("/", async (
			ClaimsPrincipal user,
			[FromServices] IQueryHandler<GetUserOrdersQuery, List<Order>> handler
		) =>
		{
			var userId = user.FindFirst("sub")?.Value ?? "";

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var result = await handler.HandleAsync(new GetUserOrdersQuery(userId));
			return result.Match(
				orderList => Results.Ok(orderList),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.NotFound => Results.NotFound(),
					Error e when e.Code == ErrorCodes.Unauthorized => Results.Unauthorized(),
					Error e when e.Code == ErrorCodes.ValidationFailed => Results.BadRequest(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("GetMyOrders");

		orders.MapPost("/", async (
			ClaimsPrincipal user,
			OrderCreateRequest request,
			[FromServices] ICommandHandler<CreateOrderCommand, Order> handler
		) =>
		{
			var userId = user.FindFirst("sub")?.Value ?? "";

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var userEmail = user.FindFirst("email")?.Value ?? "";

			var command = new CreateOrderCommand(
				userId,
				userEmail,
				request.ShippingAddress,
				request.ShippingCity,
				request.ShippingState,
				request.ShippingPostalCode,
				request.LineItems,
				request.CardNumber,
				request.CardholderName,
				request.ExpiryDate,
				request.Cvv
			);

			var result = await handler.HandleAsync(command);
			return result.Match(
				order => Results.Ok(order),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.NotFound => Results.NotFound(),
					Error e when e.Code == ErrorCodes.Unauthorized => Results.Unauthorized(),
					Error e when e.Code == ErrorCodes.ValidationFailed => Results.BadRequest(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("CreateOrder");

		// Admin endpoints for orders
		var adminOrders = api.MapGroup("/admin/orders")
			.RequireAuthorization("AdminOnly");

		adminOrders.MapGet("/pending", async (
			[FromQuery] int page,
			[FromQuery] int pageSize,
			[FromServices] IQueryHandler<GetAdminPendingOrdersQuery, PagedOrderResponse> handler
		) =>
		{
			var query = new GetAdminPendingOrdersQuery(
				page > 0 ? page : 1,
				pageSize > 0 ? pageSize : 25
			);

			var result = await handler.HandleAsync(query);
			return result.Match(
				pagedOrders => Results.Ok(pagedOrders),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("GetPendingOrders");

		adminOrders.MapPost("/{id}/accept", async (
			Guid id,
			[FromServices] ICommandHandler<AcceptOrderForFulfillmentCommand, Unit> handler
		) =>
		{
			var command = new AcceptOrderForFulfillmentCommand(id);
			var result = await handler.HandleAsync(command);
			return result.Match(
				_ => Results.NoContent(),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.NotFound => Results.NotFound(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.ValidationFailed => Results.BadRequest(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("AcceptOrderForFulfillment");

		adminOrders.MapGet("/processing-queue", async (
			[FromQuery] int? status,
			[FromQuery] string? userEmail,
			[FromQuery] string? sortBy,
			[FromQuery] bool? sortDescending,
			[FromQuery] int page,
			[FromQuery] int pageSize,
			[FromServices] IQueryHandler<GetProcessingQueueOrdersQuery, PagedOrderResponse> handler
		) =>
		{
			var query = new GetProcessingQueueOrdersQuery(
				status.HasValue ? (OrderStatus)status.Value : OrderStatus.Processing,
				userEmail,
				sortBy ?? "OrderDate",
				sortDescending ?? false,
				page > 0 ? page : 1,
				pageSize > 0 ? pageSize : 25
			);

			var result = await handler.HandleAsync(query);
			return result.Match(
				pagedOrders => Results.Ok(pagedOrders),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("GetProcessingQueueOrders");

		adminOrders.MapPost("/{id}/ship", async (
			Guid id,
			[FromServices] ICommandHandler<UpdateOrderToShippedCommand, Unit> handler
		) =>
		{
			var command = new UpdateOrderToShippedCommand(id);
			var result = await handler.HandleAsync(command);
			return result.Match(
				_ => Results.NoContent(),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.NotFound => Results.NotFound(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.ValidationFailed => Results.BadRequest(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("UpdateOrderToShipped");

		adminOrders.MapPost("/{id}/deliver", async (
			Guid id,
			[FromServices] ICommandHandler<UpdateOrderToDeliveredCommand, Unit> handler
		) =>
		{
			var command = new UpdateOrderToDeliveredCommand(id);
			var result = await handler.HandleAsync(command);
			return result.Match(
				_ => Results.NoContent(),
				error => error switch
				{
					Error e when e.Code == ErrorCodes.NotFound => Results.NotFound(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.ValidationFailed => Results.BadRequest(new { error = e.Message }),
					Error e when e.Code == ErrorCodes.DatabaseError => Results.Problem(detail: error.Message, statusCode: 500),
					_ => Results.Problem(detail: error.Message, statusCode: 500)
				}
			);
		})
		.WithName("UpdateOrderToDelivered");

		return app;
	}
}
