using CopilotDemoApp.Worker;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

// Add Azure Service Bus client
builder.AddAzureServiceBusClient("servicebus");

// Register the order processing worker
builder.Services.AddHostedService<OrderProcessor>();

// Configure OpenTelemetry
builder.Logging.AddOpenTelemetry(logging =>
{
	logging.IncludeFormattedMessage = true;
	logging.IncludeScopes = true;
});

builder.Services.AddOpenTelemetry()
	.WithMetrics(metrics =>
	{
		metrics.AddRuntimeInstrumentation()
			.AddOtlpExporter();
	})
	.WithTracing(tracing =>
	{
		tracing.AddSource(builder.Environment.ApplicationName)
			.AddSource("Azure.Messaging.ServiceBus")
			.AddOtlpExporter();
	});

var host = builder.Build();
host.Run();
