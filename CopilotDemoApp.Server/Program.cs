using CopilotDemoApp.Server;
using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Features.Order;
using CopilotDemoApp.Server.Shared;
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

app.MapProductEndpoints();
app.MapOrderEndpoints();

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();
