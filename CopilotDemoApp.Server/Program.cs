using CopilotDemoApp.Server;
using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Keycloak authentication
builder.Services
	.AddAuthentication()
	.AddKeycloakJwtBearer("keycloak", realm: "copilotdemoapp", options =>
	{
		options.Audience = "copilotdemoapp-api";

		if (builder.Environment.IsDevelopment())
		{
			options.RequireHttpsMetadata = false;
			// Accept tokens from both internal (keycloak) and external (localhost) URLs
			options.TokenValidationParameters.ValidIssuers = new[]
			{
				"http://keycloak:8080/realms/copilotdemoapp",
				"http://localhost:8080/realms/copilotdemoapp"
			};
		}

		// Map Keycloak's realm_access.roles to ASP.NET Core's role claims
		options.MapInboundClaims = false; // Use original claim names from the token
		options.TokenValidationParameters.RoleClaimType = "role";
		options.TokenValidationParameters.NameClaimType = "preferred_username";

		// Extract roles from Keycloak's nested realm_access structure
		options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
		{
			OnTokenValidated = context =>
			{
				if (context.Principal?.Identity is not System.Security.Claims.ClaimsIdentity identity)
					return Task.CompletedTask;

				// Extract roles from realm_access.roles
				var realmAccessClaim = context.Principal.FindFirst("realm_access");
				if (realmAccessClaim?.Value != null)
				{
					try
					{
						var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
						if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
						{
							foreach (var role in rolesElement.EnumerateArray())
							{
								identity.AddClaim(new System.Security.Claims.Claim("role", role.GetString() ?? ""));
							}
						}
					}
					catch
					{
						// If parsing fails, continue without roles
					}
				}

				return Task.CompletedTask;
			}
		};
	});

// Add authorization policies
builder.Services.AddAuthorizationBuilder()
	.AddPolicy("AdminOnly", policy =>
		policy.RequireRole("admin"))
	.AddPolicy("UserAccess", policy =>
		policy.RequireRole("admin", "user")); // Admin can also access user features

// Add services to the container.
builder.Services.AddProblemDetails();

// Add AppDbContext using Aspire's EF Core/Postgres integration
builder.AddNpgsqlDbContext<CopilotDemoApp.Server.Database.AppDbContext>("appdb");

builder.Services.AddCqrsHandlers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
	// Apply EF Core migrations at startup
	using (var scope = app.Services.CreateScope())
	{
		var db = scope.ServiceProvider.GetRequiredService<CopilotDemoApp.Server.Database.AppDbContext>();
		db.Database.Migrate();
	}
	app.MapOpenApi();
}


string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];


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
		error => Results.Problem(detail: error.Message, statusCode: 500)
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
		error => Results.Problem(detail: error.Message, statusCode: 500)
	);
})
.WithName("GetProductById")
.AllowAnonymous();

// Admin endpoints for product CRUD operations
var adminProducts = api.MapGroup("/admin/products")
	.RequireAuthorization("AdminOnly");

adminProducts.MapPost("/", (ProductCreateRequest request) =>
{
	// TODO: Implement product creation
	return Results.Created($"/api/products/{Guid.NewGuid()}", new { message = "Product created (placeholder)" });
})
.WithName("CreateProduct");

adminProducts.MapPut("/{id:guid}", (Guid id, ProductUpdateRequest request) =>
{
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

// User endpoints for orders
var orders = api.MapGroup("/orders")
	.RequireAuthorization("UserAccess");

orders.MapGet("/", () =>
{
	// TODO: Implement get user orders
	return Results.Ok(new[]
	{
		new { id = Guid.NewGuid(), date = DateTime.UtcNow, total = 99.99m, status = "Completed" },
		new { id = Guid.NewGuid(), date = DateTime.UtcNow.AddDays(-7), total = 149.99m, status = "Shipped" }
	});
})
.WithName("GetMyOrders");

orders.MapGet("/{id:guid}", (Guid id) =>
{
	// TODO: Implement get order by id
	return Results.Ok(new { id, date = DateTime.UtcNow, total = 99.99m, status = "Completed", items = new[] { new { productId = Guid.NewGuid(), name = "Sample Product", quantity = 1, price = 99.99m } } });
})
.WithName("GetOrderById");

orders.MapPost("/", (OrderCreateRequest request) =>
{
	// TODO: Implement order creation
	return Results.Created($"/api/orders/{Guid.NewGuid()}", new { message = "Order created (placeholder)" });
})
.WithName("CreateOrder");

api.MapGet("weatherforecast", () =>
{
	var forecast = Enumerable.Range(1, 5).Select(index =>
		new WeatherForecast
		(
			DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
			Random.Shared.Next(-20, 55),
			summaries[Random.Shared.Next(summaries.Length)]
		))
		.ToArray();
	return forecast;
})
.WithName("GetWeatherForecast");

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();

// Request/Response DTOs for new endpoints
record ProductCreateRequest(string Name, string Description, decimal Price, bool IsActive);
record ProductUpdateRequest(string Name, string Description, decimal Price, bool IsActive);
record OrderCreateRequest(Guid[] ProductIds);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
