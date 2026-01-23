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
		options.TokenValidationParameters.NameClaimType = "sub"; // Use standard OIDC subject identifier

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

// Configure HttpClient for OTLP relay to accept development certificates
builder.Services.AddHttpClient(string.Empty, client => { })
	.ConfigurePrimaryHttpMessageHandler(() =>
	{
		return new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
		};
	});

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

// OTLP relay endpoint - forwards browser telemetry (HTTP/1.1) to Aspire's OTLP endpoint (HTTP/2)
app.MapPost("/api/otlp/{**path}", async (HttpContext context, IHttpClientFactory httpClientFactory, string? path) =>
{
	var otlpEndpoint = app.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
	if (string.IsNullOrEmpty(otlpEndpoint))
		return Results.BadRequest("OTLP endpoint not configured");

	var otlpHeaders = app.Configuration["OTEL_EXPORTER_OTLP_HEADERS"];

	app.Logger.LogInformation("OTLP relay received request with path: '{Path}', headers: '{Headers}', content-type: '{ContentType}'", 
		path ?? "(empty)", 
		otlpHeaders ?? "(none)",
		context.Request.ContentType ?? "(none)");

	var client = httpClientFactory.CreateClient();
	var targetUrl = string.IsNullOrEmpty(path) 
		? otlpEndpoint.TrimEnd('/')
		: $"{otlpEndpoint.TrimEnd('/')}/{path}";

	using var requestMessage = new HttpRequestMessage(HttpMethod.Post, targetUrl);
	requestMessage.Version = new Version(2, 0);
	
	// Read and buffer the request body
	using var memoryStream = new MemoryStream();
	await context.Request.Body.CopyToAsync(memoryStream);
	memoryStream.Position = 0;
	requestMessage.Content = new StreamContent(memoryStream);

	// Copy content-type header
	if (context.Request.ContentType != null)
	{
		requestMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
		app.Logger.LogInformation("Set Content-Type: {ContentType}", context.Request.ContentType);
	}
	
	// Copy other relevant headers from the original request
	foreach (var header in context.Request.Headers)
	{
		if (header.Key.StartsWith("x-", StringComparison.OrdinalIgnoreCase) || 
			header.Key.Equals("user-agent", StringComparison.OrdinalIgnoreCase))
		{
			requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
			app.Logger.LogInformation("Copied header from request: {HeaderName}", header.Key);
		}
	}

	// Add OTLP authentication headers
	if (!string.IsNullOrEmpty(otlpHeaders))
	{
		foreach (var header in otlpHeaders.Split(','))
		{
			var parts = header.Split('=', 2);
			if (parts.Length == 2)
			{
				var headerName = parts[0].Trim();
				var headerValue = parts[1].Trim();
				requestMessage.Headers.TryAddWithoutValidation(headerName, headerValue);
				app.Logger.LogInformation("Added header: {HeaderName} = {HeaderValue}", headerName, headerValue);
			}
		}
	}

	try
	{
		var response = await client.SendAsync(requestMessage);
		
		if (!response.IsSuccessStatusCode)
		{
			var responseBody = await response.Content.ReadAsStringAsync();
			app.Logger.LogWarning("OTLP relay failed: {StatusCode}, Response: {ResponseBody}", response.StatusCode, responseBody);
		}
		
		app.Logger.LogInformation("OTLP relay forwarded to {TargetUrl}, response status: {StatusCode}", targetUrl, response.StatusCode);
		return Results.StatusCode((int)response.StatusCode);
	}
	catch (Exception ex)
	{
		app.Logger.LogError(ex, "Failed to forward OTLP telemetry to {TargetUrl}", targetUrl);
		return Results.Problem("Failed to forward telemetry");
	}
});

app.MapDefaultEndpoints();

app.UseFileServer();

app.Run();
