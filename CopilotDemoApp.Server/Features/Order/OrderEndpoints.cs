using CopilotDemoApp.Server.DTOs;
using CopilotDemoApp.Server.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CopilotDemoApp.Server.Features.Order;

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
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? user.FindFirst("sub")?.Value
				?? "";

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
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
				?? user.FindFirst("sub")?.Value
				?? "";

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			var userEmail = user.FindFirst(ClaimTypes.Email)?.Value
				?? user.FindFirst("email")?.Value
				?? "";

			var command = new CreateOrderCommand(
				userId,
				userEmail,
				request.ShippingAddress,
				request.ShippingCity,
				request.ShippingState,
				request.ShippingPostalCode,
				request.LineItems
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

		return app;
	}
}
