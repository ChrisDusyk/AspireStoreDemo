var builder = DistributedApplication.CreateBuilder(args);

// Add PostgreSQL server and database
var postgres = builder
	.AddPostgres("postgres")
	.WithDataVolume()
	.WithPgAdmin();
var db = postgres.AddDatabase("appdb");
var keycloakdb = postgres.AddDatabase("keycloakdb");

// Add Keycloak with realm import
var keycloak = builder
	.AddKeycloak("keycloak", port: 8080)
	.WithRealmImport("./copilotdemoapp-realm.json")
	.WithEnvironment("KC_DB", "postgres")
	.WithEnvironment("KC_DB_URL", ReferenceExpression.Create($"jdbc:postgresql://{postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{postgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}/keycloakdb"))
	.WithEnvironment("KC_DB_USERNAME", postgres.Resource.UserNameReference)
	.WithEnvironment("KC_DB_PASSWORD", postgres.Resource.PasswordParameter)
	.WaitFor(keycloakdb);

var server = builder.AddProject<Projects.CopilotDemoApp_Server>("server")
	.WithReference(db).WaitFor(db)
	.WithReference(keycloak).WaitFor(keycloak)
	.WithHttpHealthCheck("/health")
	.WithExternalHttpEndpoints();

var webfrontend = builder.AddViteApp("webfrontend", "../frontend")
	.WithEndpoint("http", (endpointAnnotation) =>
	{
		endpointAnnotation.Port = 5173;
		endpointAnnotation.IsExternal = true;
		endpointAnnotation.IsProxied = false;
	})
	.WithReference(server)
	.WaitFor(server);

server.PublishWithContainerFiles(webfrontend, "wwwroot");

builder.Build().Run();
