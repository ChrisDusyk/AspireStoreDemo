using CopilotDemoApp.Server.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CopilotDemoApp.Server.Features.Product;

// Product DTOs
public record ProductCreateRequest(string Name, string Description, decimal Price, bool IsActive);
public record ProductUpdateRequest(string Name, string Description, decimal Price, bool IsActive);

public static class ProductEndpoints
{
	public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
	{
		var api = app.MapGroup("/api");
		var products = api.MapGroup("/products");

		products.MapGet("/", async (
			[AsParameters] ProductFilterRequest filter,
			[FromServices] IQueryHandler<GetProductsQuery, PagedProductResponse> handler
		) =>
		{
			var result = await handler.HandleAsync(new GetProductsQuery(filter));
			return result.Match(
				paged => Results.Ok(paged),
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
		.WithName("GetProducts")
		.AllowAnonymous();

		products.MapGet("/{id:guid}", async (
			Guid id,
			[FromServices] IQueryHandler<GetProductByIdQuery, Option<ProductResponse>> handler
		) =>
		{
			var result = await handler.HandleAsync(new GetProductByIdQuery(id));
			return result.Match(
				opt => opt.Match(
					product => Results.Ok(product),
					() => Results.NotFound()
				),
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
		.WithName("GetProductById")
		.AllowAnonymous();

		// Admin endpoints for product CRUD operations
		var adminProducts = api.MapGroup("/admin/products")
			.RequireAuthorization("AdminOnly");

		adminProducts.MapGet("/", async (
			[AsParameters] Admin.AdminProductFilterRequest filter,
			[FromServices] IQueryHandler<Admin.GetAllProductsQuery, PagedProductResponse> handler
		) =>
		{
			var result = await handler.HandleAsync(new Admin.GetAllProductsQuery(filter));
			return result.Match(
				paged => Results.Ok(paged),
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
		.WithName("GetAllProductsAdmin");

		adminProducts.MapPost("/", (ClaimsPrincipal user, ProductCreateRequest request) =>
		{
			var userId = user.FindFirst("sub")?.Value ?? "";

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			// TODO: Implement product creation
			return Results.Created($"/api/products/{Guid.NewGuid()}", new { message = "Product created (placeholder)" });
		})
		.WithName("CreateProduct");

		adminProducts.MapPut("/{id:guid}", (ClaimsPrincipal user, Guid id, ProductUpdateRequest request) =>
		{
			var userId = user.FindFirst("sub")?.Value ?? "";

			if (string.IsNullOrEmpty(userId))
				return Results.Unauthorized();

			// TODO: Implement product update
			return Results.Ok(new { message = $"Product {id} updated (placeholder)" });
		})
		.WithName("UpdateProduct");

		adminProducts.MapDelete("/{id:guid}", (Guid id) =>
		{
			// TODO: Implement product deletion
			return Results.NoContent();
		})
		.WithName("DeleteProduct");

		return app;
	}
}
