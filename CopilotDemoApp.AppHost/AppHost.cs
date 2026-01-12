var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server and database
var postgres = builder
	.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();
var db = postgres.AddDatabase("appdb");

// Add Keycloak with realm import
var keycloak = builder
	.AddKeycloak("keycloak", port: 8080)
	.WithDataVolume()
	.WithRealmImport("./copilotdemoapp-realm.json");

var server = builder.AddProject<Projects.CopilotDemoApp_Server>("server")
	.WithReference(db).WaitFor(db)
	.WithReference(keycloak).WaitFor(keycloak)
	.WithHttpHealthCheck("/health")
	.WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
	.WithReference(server)
	.WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
