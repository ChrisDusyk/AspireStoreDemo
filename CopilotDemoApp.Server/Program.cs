using CopilotDemoApp.Server;
using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();


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
.WithName("GetProducts");

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
.WithName("GetProductById");

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

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
