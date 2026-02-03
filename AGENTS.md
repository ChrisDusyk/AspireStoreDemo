# Project Preferences and Patterns

## Coding Conventions

- **C#**: Use tab indentation (size 4), file-scoped namespaces, and modern C# conventions (target-typed new, nullability, etc.).
- **TypeScript/JavaScript/JSON/Markdown**: Use space indentation (size 2).
- **HTML/CSS**: Use space indentation (size 4).
- **Trailing whitespace**: Always trimmed.
- **EOF newlines**: Always present.

## Functional Patterns

- Prefer functional coding patterns in C#.
- Use a generic `Result<T>` class for railway-oriented programming (ROP), supporting chaining (`Bind`, `Map`, etc.).
- Use a generic `Option<T>` class instead of nulls in domain code.
- Use a `Unit` struct to represent void results.
- Use an `Error` class with string error codes and support for exceptions or domain failures.

## CQRS Infrastructure

- Use async CQRS patterns:
  - Marker interfaces: `IQuery<TResult>`, `ICommand<TResult>`
  - Handler interfaces: `IQueryHandler<TQuery, TResult>`, `ICommandHandler<TCommand, TResult>` (all async, returning `Task<Result<TResult>>`)
  - Handlers are registered in DI via a generic extension method.
- Domain-focused queries and commands are preferred for retrieving or modifying data.

## Project Organization

- For the Server project, organize domain logic into subfolders under a `Features` folder at the project root.
- Shared infrastructure (e.g., Result, Option, CQRS interfaces) goes in a `Shared` folder at the project root.

## Endpoint Organization

- Minimal API endpoint declarations are organized into feature-specific static extension methods.
- Each feature folder contains an `*Endpoints.cs` file (e.g., `Features/Product/ProductEndpoints.cs`, `Features/Order/OrderEndpoints.cs`).
- API DTOs (request/response records) are defined in the same file as their endpoints, keeping all API contract code together.
- Extension methods follow the pattern `Map{Feature}Endpoints(this IEndpointRouteBuilder app)` and return `IEndpointRouteBuilder` for chaining.
- Extension methods create their own route groups and register all endpoints for that feature.
- Program.cs calls these extension methods (e.g., `app.MapProductEndpoints()`, `app.MapOrderEndpoints()`) to register endpoints.
- This pattern follows vertical slice architecture, keeping all feature code (DTOs, endpoints, handlers, commands, queries) together.

## Testing

- All infrastructure and domain types have matching unit tests in the test project, mirroring the folder structure of the main project.
- Use xUnit v3 and standard C# test naming conventions.
- Use the null-forgiving operator (`!`) when accessing `Result<T>.Value` or `Result<T>.Error` properties after asserting success/failure. Example: `Assert.Equal(expected, result.Value!.Property)` or `Assert.Equal(ErrorCodes.NotFound, result.Error!.Code)`.

---

# Copilot instructions

## Frontend (React/Vite) Styling and Structure

---

## React Component Declaration Style

- All React component declarations must use function declaration syntax (e.g., `function MyComponent() { ... }`).
- All handlers, callbacks, and inline render functions should use anonymous arrow functions (e.g., `onClick={() => ...}` or `const handle = () => ...`).
- Do not use arrow functions for the main component declaration.

# API Pagination Strategy

## Product Endpoints

- All product listing endpoints use query string parameters for filtering and pagination:
  - `name` (optional, partial match, case-insensitive)
  - `isActive` (optional, defaults to true)
  - `page` (optional, defaults to 1)
  - `pageSize` (optional, defaults to 25)

- The API response includes:
  - `products`: List of products for the current page
  - `totalCount`: Total number of products matching the filter
  - `page`: Current page number
  - `pageSize`: Number of products per page
  - `totalPages`: Total number of pages, calculated as `Math.Ceiling(totalCount / (double)pageSize)` (always rounds up)

- Inactive products are excluded by default unless `isActive=false` is specified.

- Pagination metadata is included in all paged responses for future extensibility.

- All UI styling uses [Tailwind CSS v4](https://tailwindcss.com/) via the @tailwindcss/vite integration. No custom CSS or PostCSS is used.
- Page components must be placed in the `frontend/src/pages` folder. Each route should correspond to a page component in this directory.
- Common, reusable UI components should be placed in the `frontend/src/components` folder.
- Only Tailwind utility classes are used for layout and design. No custom CSS files or variables are present.

This repository is set up to use Aspire. Aspire is an orchestrator for the entire application and will take care of configuring dependencies, building, and running the application. The resources that make up the application are defined in `apphost.cs` including application code and external dependencies.

## General recommendations for working with Aspire

1. Before making any changes always run the apphost using `aspire run` and inspect the state of resources to make sure you are building from a known state.
1. Changes to the _apphost.cs_ file will require a restart of the application to take effect.
1. Make changes incrementally and run the aspire application using the `aspire run` command to validate changes.
1. Use the Aspire MCP tools to check the status of resources and debug issues.

## Running the application

To run the application run the following command:

```
aspire run
```

If there is already an instance of the application running it will prompt to stop the existing instance. You only need to restart the application if code in `apphost.cs` is changed, but if you experience problems it can be useful to reset everything to the starting state.

## Checking resources

To check the status of resources defined in the app model use the _list resources_ tool. This will show you the current state of each resource and if there are any issues. If a resource is not running as expected you can use the _execute resource command_ tool to restart it or perform other actions.

## Listing integrations

IMPORTANT! When a user asks you to add a resource to the app model you should first use the _list integrations_ tool to get a list of the current versions of all the available integrations. You should try to use the version of the integration which aligns with the version of the Aspire.AppHost.Sdk. Some integration versions may have a preview suffix. Once you have identified the correct integration you should always use the _get integration docs_ tool to fetch the latest documentation for the integration and follow the links to get additional guidance.

## Debugging issues

IMPORTANT! Aspire is designed to capture rich logs and telemetry for all resources defined in the app model. Use the following diagnostic tools when debugging issues with the application before making changes to make sure you are focusing on the right things.

1. _list structured logs_; use this tool to get details about structured logs.
2. _list console logs_; use this tool to get details about console logs.
3. _list traces_; use this tool to get details about traces.
4. _list trace structured logs_; use this tool to get logs related to a trace

## Other Aspire MCP tools

1. _select apphost_; use this tool if working with multiple app hosts within a workspace.
2. _list apphosts_; use this tool to get details about active app hosts.

## Playwright MCP server

The playwright MCP server has also been configured in this repository and you should use it to perform functional investigations of the resources defined in the app model as you work on the codebase. To get endpoints that can be used for navigation using the playwright MCP server use the list resources tool.

## Updating the app host

The user may request that you update the Aspire apphost. You can do this using the `aspire update` command. This will update the apphost to the latest version and some of the Aspire specific packages in referenced projects, however you may need to manually update other packages in the solution to ensure compatibility. You can consider using the `dotnet-outdated` with the users consent. To install the `dotnet-outdated` tool use the following command:

```
dotnet tool install --global dotnet-outdated-tool
```

## Persistent containers

IMPORTANT! Consider avoiding persistent containers early during development to avoid creating state management issues when restarting the app.

## Aspire workload

IMPORTANT! The aspire workload is obsolete. You should never attempt to install or use the Aspire workload.

## Official documentation

IMPORTANT! Always prefer official documentation when available. The following sites contain the official documentation for Aspire and related components

1. https://aspire.dev
2. https://learn.microsoft.com/dotnet/aspire
3. https://nuget.org (for specific integration package details)

## CopilotDemoApp.Server changes

This is an ASP.NET Web Api using the minimal API pattern.
