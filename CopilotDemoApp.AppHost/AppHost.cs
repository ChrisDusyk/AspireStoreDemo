var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server and database
var postgres = builder
	.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();
var db = postgres.AddDatabase("appdb");

var server = builder.AddProject<Projects.CopilotDemoApp_Server>("server")
	.WithReference(db).WaitFor(db)
	.WithHttpHealthCheck("/health")
	.WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
	.WithReference(server)
	.WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
